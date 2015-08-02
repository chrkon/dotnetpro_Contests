using System;
using System.Linq;
using System.Collections.Generic;
using contest.submission.contract;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using System.Diagnostics;

// 
// Lösung für den Programmierwettbewerb der dotNetPro 2015/07
// Programiert von Christof Konstantinopoulos 
//
// e-Mail: christof@konstantinopoulos.de
//
// Der Quelltext unterliegt dem Urheberrecht.
// Die Nutzung ist beschränkt auf Informations- und Ausbildungszwecke.
//
// Für die Lösung wurde das Paket Math.net (http://numerics.mathdotnet.com/) genutzt. 
//
// 21.7.2015

namespace contest.submission
{
  [Serializable]
  public class Solution : IDnp1507Solution
  {
    private Products allproducts;
    private ProductPackages allproductpackages;
    
    private Matrix<double> productMatrix; // die Zeilen definieren ein Paket, die Reihen stehen für ein Produkt
    private Vector<double> prices; // Preise für jedes Produkt/Paket
    private Vector<double> orderedProducts; // Bestellung (Besteht nur aus Produkten)
    private Vector<double> packagedOrder; // Bestellung aufgeschlüsselt in Pakete und Produkte

    private int productsAndPackagesCount = 0; 

    public void Setup(Products products, ProductPackages productpackages)
    {
      this.allproducts = products;
      this.allproductpackages = productpackages;

      productsAndPackagesCount = products.Count + productpackages.Count;
    
      var product2PackageArray = new double[productsAndPackagesCount, products.Count];
      var pricesArray = new double[productsAndPackagesCount];
      var packagedOrderArray = new double[productsAndPackagesCount];

      // Produktmatrix und Preisvectoraufbauen

      var col = 0;
      var row = 0;

      var grundpreis = 0.0;

      var productSet = new HashSet<Tuple<double, double, object>>();    

      // Schritt 1 - Preis Index der Pakete ermitteln
      foreach (var package in productpackages)
      {
          grundpreis = 0.0;
          foreach (var price in package.Products)
          {
              var preis = products[price];
              grundpreis += preis;
          }
          var preisIndex = (package.Price / grundpreis);
          productSet.Add(new Tuple<double, double, object>(preisIndex, package.Price, package));
      }

      // Schritt 2 - Preis Index der Produkte ermitteln
      foreach (var product in products)
      {
          grundpreis = product.Value;
          var preisIndex = (product.Value / grundpreis);
          productSet.Add(new Tuple<double, double, object>(preisIndex, product.Value, product));
      }

      // Schritt 3 - das ProduktSet sortieren und in eine Matrix überführen

      var query = from p in productSet
                  orderby p.Item1 ascending
                  select p;
        
        row = 0;
        foreach (var p in query)
        {
            col = 0;
            foreach (var product in products)
            {
                if (p.Item3 is ProductPackage)
                {
                    foreach (var item in ((ProductPackage)p.Item3).Products)
                    {
                        if (item == product.Key)
                        {
                            product2PackageArray[row, col]++;
                        }
                    }
                }
                else
                {
                    if (product.Key == ((KeyValuePair<string, int>)p.Item3).Key)
                    {
                        product2PackageArray[row, col]++;
                    }
                }
                col++;
            }
            pricesArray[row] = p.Item2;
            row++;
        }

        productMatrix = Matrix<double>.Build.DenseOfArray(product2PackageArray);
        packagedOrder = Vector<double>.Build.DenseOfArray(packagedOrderArray);
        prices = Vector<double>.Build.DenseOfArray(pricesArray);
        orderedProducts = Vector<double>.Build.Dense(products.Count);
    }
    
    public void Order(string productname)
    {
        if (!allproducts.ContainsKey(productname)) return;

        // Bestelltes Produkt in orderedProducts einbauen
        var col = 0;
        foreach (var product in allproducts)
        {
            if (product.Key == productname)
            {
                orderedProducts[col]++;
            }
            col++;
        }

        Optimize();

        if (SendPrice != null) SendPrice(CalculateTotalPrice());
    }

    private void Optimize()
    {
        var minimumList = new List<Tuple<double, Vector<Double>>>();

        for (int p = 0; p < productsAndPackagesCount; p++)
        {
            var order = Vector<Double>.Build.DenseOfVector(orderedProducts);
            var actualPackagedOrder = Vector<Double>.Build.Dense(productsAndPackagesCount);

            var runs = 0;
            var maxRuns = productMatrix.Row(p).Sum();

            while (order.Sum() > 0 && runs <= maxRuns )
            {
                runs++;

                var a = productMatrix.Multiply(order);

                for (int i = 0; i < productsAndPackagesCount; i++)
                {
                    if (allproductpackages.Count>1 && i == p) continue; 

                    if (a[i] > 0)
                    {
                        var row = productMatrix.Row(i);

                        // Paket erkennen 
                        while (OrderContainsProductCombination(order, row))
                        {
                            // Paket hinzufügen
                            actualPackagedOrder[i]++;

                            // zugehöhrige Produkte aus order entfernen
                            for (int j = 0; j < order.Count; j++)
                            {
                                var itemCount = productMatrix.Row(i)[j];
                                if (itemCount > 0 && order[j] >= itemCount)
                                {
                                    order[j] -= itemCount;
                                }                                
                            }
                        }
                    }
                }
            }
            var price = actualPackagedOrder.PointwiseMultiply(prices).Sum();
            if (runs <= maxRuns) minimumList.Add(new Tuple<double, Vector<double>>(price, actualPackagedOrder));
        }
        // Kombination mit dem gerigsten Preis verwenden.
        var obj = minimumList.OrderBy(item => item.Item1).FirstOrDefault();
        if (obj != null) packagedOrder = obj.Item2;
    }

    private bool OrderContainsProductCombination(Vector<double> order, Vector<double> packageContent)
    {
        var result = true;
        for (int i  = 0; i < orderedProducts.Count; i++)
        {
            if (order[i] < packageContent[i]) result = false;
        }
        return result;
    }

    private int CalculateTotalPrice()
    {
        var preis =packagedOrder.PointwiseMultiply(prices).Sum();
        Debug.WriteLine("Preis = {0}", preis);
        return (int)preis;
    }

    public event Action<int> SendPrice;
  }
}
