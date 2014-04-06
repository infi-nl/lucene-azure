//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Web.Http;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Infi.WebApi;
//using Infi.WebApi.Controllers;

//namespace Infi.WebApi.Tests.Controllers
//{
//    [TestClass]
//    public class SearchControllerTest
//    {
//        [TestMethod]
//        public void Get()
//        {
//            // Arrange
//            SearchController controller = new SearchController();

//            // Act
//            IEnumerable<string> result = controller.Get();

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual(2, result.Count());
//            Assert.AreEqual("value1", result.ElementAt(0));
//            Assert.AreEqual("value2", result.ElementAt(1));
//        }

//        [TestMethod]
//        public void GetById()
//        {
//            // Arrange
//            SearchController controller = new SearchController();

//            // Act
//            string result = controller.Get(5);

//            // Assert
//            Assert.AreEqual("value", result);
//        }

//        [TestMethod]
//        public void Post()
//        {
//            // Arrange
//            SearchController controller = new SearchController();

//            // Act
//            controller.Post("value");

//            // Assert
//        }

//        [TestMethod]
//        public void Put()
//        {
//            // Arrange
//            SearchController controller = new SearchController();

//            // Act
//            controller.Put(5, "value");

//            // Assert
//        }

//        [TestMethod]
//        public void Delete()
//        {
//            // Arrange
//            SearchController controller = new SearchController();

//            // Act
//            controller.Delete(5);

//            // Assert
//        }
//    }
//}
