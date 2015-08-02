using System;
using System.Collections.Generic;
using System.Linq;
using contest.submission.contract;
using contest.submission.Tests.Helper;
using FluentAssertions;
 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
 
//
// Die Tests wurden zum Teil aus dem Forum der DotNetPro übernommen
// - Vielen Dank an M.Beetz
//

namespace contest.submission.Tests
{
    [TestClass]
    public class SolutionTests
    {
        [TestMethod]
        public void Test_CK_einProdukt()
        {
            var sut = new Solution();

            var products = new Products { { "Apfel", 10 } };

            var success = false;

            sut.Setup(products, new ProductPackages());

            var price = 0;
            sut.SendPrice += p =>
            {
                price = p;
                Debug.WriteLine("{0},", price);
            };

            sut.Order("Apfel");
            price.Should().Be(10);
        }

        [TestMethod]
        public void Test_CK_einProduktZweiMal()
        {
            var sut = new Solution();

            var products = new Products { { "Apfel", 10 } };

            var success = false;

            sut.Setup(products, new ProductPackages());

            var price = 0;
            sut.SendPrice += p =>
            {
                price = p;
                Debug.WriteLine("{0},", price);
            };

            sut.Order("Apfel");
            price.Should().Be(10);
            sut.Order("Apfel");
            price.Should().Be(20);
        }

        [TestMethod]
        public void Test_CK_einProduktZweiMal_InsPaket()
        {
            var sut = new Solution();

            var products = new Products { { "Apfel", 10 } };

            var success = false;

            var productPackages = new ProductPackages
            {
                new ProductPackage(){ Price = 15, Products = { "Apfel", "Apfel" }},

            };

            sut.Setup(products, productPackages);

            var price = 0;
            sut.SendPrice += p =>
            {
                price = p;
                Debug.WriteLine("{0},", price);
            };

            sut.Order("Apfel");
            price.Should().Be(10);
            sut.Order("Apfel");
            price.Should().Be(15);
        }

        
        [TestMethod]
        public void Test_Forum()
        {
            var sut = new Solution();

            var products = new Products {
                { "VegMac", 4 },
                { "BurgerMac", 1 },
                { "WoodMac", 3 },
                { "MacMac", 5 },
                { "PommesPommes", 2 },
                { "Pommes", 1 },
                { "PommesGr", 1 },
                { "Cola", 3 }
            };

            var productPackages = new ProductPackages
            {
                new ProductPackage(){ Price = 1, Products = { "MacMac", "Cola" }},
                new ProductPackage(){ Price = 10, Products = { "BurgerMac", "WoodMac", "Pommes", "Cola" }},
                new ProductPackage(){ Price = 7, Products = { "VegMac", "Cola", "Pommes" }},
                new ProductPackage(){ Price = 7, Products = { "VegMac", "Cola", "PommesGr" }},
                new ProductPackage(){ Price = 2, Products = { "Cola", "PommesGr" }},
                new ProductPackage(){ Price = 1, Products = { "VegMac", "Cola", "Pommes", "Pommes" }}
            };



            sut.Setup(products, productPackages);

            var price = 0;
            sut.SendPrice += p =>
            {
                price = p;
                Debug.WriteLine("{0},",price);
            };

            sut.Order("VegMac");
            price.Should().Be(4);
            sut.Order("VegMac");
            price.Should().Be(8);
            sut.Order("VegMac");
            price.Should().Be(12);
            sut.Order("Pommes");
            price.Should().Be(13);
            sut.Order("Cola");
            price.Should().Be(15); // P3 + 2xVegMac
            sut.Order("Pommes");
            price.Should().Be(9); // P3 + P6 + 1xVegMac
            sut.Order("PommesGr");
            price.Should().Be(10); // P4 + P6 + 1xPommes
            sut.Order("MacMac");
            price.Should().Be(15); // P4 + P6 + 1xPommes + 1xMacMac
            sut.Order("WoodMac");
            price.Should().Be(18); // P4 + P6 + 1xPommes + 1xMacMac + 1xWoodMac
            sut.Order("BurgerMac");
            price.Should().Be(19); // P4 + P6 + 1xPommes + 1xMacMac + 1xWoodMac + 1xBurgeMac
            sut.Order("MacMac");
            price.Should().Be(24); // P4 + P6 + 1xPommes + 2xMacMac + 1xWoodMac + 1xBurgeMac
            sut.Order("PommesPommes");
            price.Should().Be(26); // P4 + P6 + 1xPommes + 2xMacMac + 1xWoodMac + 1xBurgeMac + PommesPommes
            sut.Order("Pommes");
            price.Should().Be(27); // P4 + P6 + 2xPommes + 2xMacMac + 1xWoodMac + 1xBurgeMac + PommesPommes
            sut.Order("Cola");
            price.Should().Be(23); // P1 + P4 + P6 + 2xPommes + 1xMacMac + 1xWoodMac + 1xBurgeMac + PommesPommes
        
        }


        [TestMethod]
        public void Test_switch_to_another_package()
        {
            var sut = new Solution();
 
            var products = new Products {{"A", 10}, {"B", 20}, {"C", 30}};
            var productPackages = new ProductPackages
            {
                new ProductPackage {Price = 29, Products = new List<string> {"A", "B"}},
                new ProductPackage {Price = 48, Products = new List<string> {"B", "C"}},
            };
 
            sut.Setup(products, productPackages);
 
            var price = 0;
            sut.SendPrice += p => price = p;
 
            sut.Order("A");
            sut.Order("B");
            price.Should().Be(29, "there is a package for A and B with price 29.");
 
            sut.Order("C");
 
            price.Should().Be(10 + 48, "it is cheaper to take package B & C");
        }
 
        [TestMethod]
        public void Test_if_pitfall_package_is_ignored()
        {
            var sut = new Solution();
 
            var products = new Products {{"A", 10}};
            var productPackages = new ProductPackages
            {
                new ProductPackage {Price = 21, Products = new List<string> {"A", "A"}}
            };
 
            sut.Setup(products, productPackages);
 
            var price = 0;
            sut.SendPrice += p => price = p;
 
            sut.Order("A");
            price.Should().Be(10);
 
            sut.Order("A");
            price.Should().Be(2 * 10, "package is a pitfall");
        }
 
        [TestMethod]
        public void Test_quantity_discount()
        {
            var sut = new Solution();
 
            var products = new Products {{"A", 10}};
            var productPackages = new ProductPackages
            {
                new ProductPackage {Price = 19, Products = new List<string> {"A", "A"}},
                new ProductPackage {Price = 27, Products = new List<string> {"A", "A", "A"}},
            };
 
            sut.Setup(products, productPackages);
 
            var price = 0;
            sut.SendPrice += p => price = p;
 
            sut.Order("A");
            price.Should().Be(10);
 
            sut.Order("A");
            price.Should().Be(19);
 
            sut.Order("A");
            price.Should().Be(27);
 
            sut.Order("A");
            price.Should().Be(27 + 10);
 
            sut.Order("A");
            price.Should().Be(27 + 19);
 
            sut.Order("A");
            price.Should().Be(2 * 27);
        }
 
        [TestMethod]
        public void Test_impossible_product()
        {
            var sut = new Solution();
 
            var products = new Products {{"A", 10}, {"B", 20}, {"C", 30}};
 
            var success = false;
 
            sut.Setup(products, new ProductPackages());
 
            sut.SendPrice += p => success = true;
 
            sut.Order("D");
 
            success.Should().BeFalse();
        }
 
        private int count = 0;

        [TestMethod]       
        public void Test_performance()
        {
            var sut = new Solution();
 
            var products = new Products {{"A", 10}, {"B", 20}, {"C", 30}};
            var productPackages = new ProductPackages
            {
                new ProductPackage {Price = 29, Products = new List<string> {"A", "B"}},
                new ProductPackage {Price = 30, Products = new List<string> {"A", "A", "A", "A"}},
                new ProductPackage {Price = 48, Products = new List<string> {"B", "C"}},
                new ProductPackage {Price = 57, Products = new List<string> {"A", "B", "C"}}
            };
 
            sut.Setup(products, productPackages);
            sut.SendPrice += price => Console.WriteLine("{0,3}. Order, Price: {1,5}", count++, price);

            //sut.OrderMultiple(50, "A");
            //sut.OrderMultiple(5, "B");
            //sut.OrderMultiple(3, "C");
            sut.OrderMultiple(40, "A");
            sut.OrderMultiple(40, "B");
            sut.OrderMultiple(40, "C");

        }
 
        [TestMethod]
        public void Test_order_where_it_is_not_better_to_take_highest_discount()
        {
            var sut = new Solution();
 
            var products = new Products { { "A", 10 } };
            var productPackages = new ProductPackages
            {
                new ProductPackage {Price = 47, Products = new List<string> {"A", "A", "A", "A", "A"}}, // discount 3/5 pp
                new ProductPackage {Price = 38, Products = new List<string> {"A", "A", "A", "A"}} // discount 1/2 pp
            };
 
            var price = 0;
            sut.Setup(products, productPackages);
            sut.SendPrice += p => price = p;
 
            sut.OrderMultiple(8, "A");
 
            price.Should().Be(2 * 38);
        }
 
        [TestMethod]
        public void Test_order_single_product_twice()
        {
            var sut = new Solution();
 
            var products = new Products {{"A", 10}};
            var price = 0;
 
            sut.Setup(products, new ProductPackages());
 
            sut.SendPrice += p => price = p;
 
            sut.Order("A");
            sut.Order("A");
 
            price.Should().Be(20);
        }
 
        [TestMethod]
        public void Test_order_with_too_big_package()
        {
            var sut = new Solution();
 
            var products = new Products {{"A", 1}};
            var price = 0;
 
            sut.Setup(products, new ProductPackages {new ProductPackage {Price = 6, Products = Enumerable.Repeat("A", 8).ToList()}});
 
            sut.SendPrice += p => price = p;
 
            sut.OrderMultiple(7, "A");
 
            price.Should().Be(7, "6 would be to many As");
        }
 
        [TestMethod]
        public void Test_order_single_product()
        {
            var sut = new Solution();
 
            var products = new Products {{"A", 10}};
            var price = 0;
 
            sut.Setup(products, new ProductPackages());
 
            sut.SendPrice += p => price = p;
 
            sut.Order("A");
 
            price.Should().Be(10);
        }
    }
}
 
 
namespace contest.submission.Tests.Helper
{
    public static class SolutionExtension
    {
        public static void OrderMultiple(this Solution solution, int quantity, string productName)
        {
            for (int i = 0; i < quantity; i++)
                solution.Order(productName);
        }
    }
}

