-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: company
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `company`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `company` /*!40100 DEFAULT CHARACTER SET utf8mb3 */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `company`;

--
-- Table structure for table `countries`
--

DROP TABLE IF EXISTS `countries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `countries` (
  `idCountry` char(3) NOT NULL,
  `couName` varchar(120) NOT NULL,
  `couTaxBase` decimal(4,2) NOT NULL,
  PRIMARY KEY (`idCountry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `countries`
--

LOCK TABLES `countries` WRITE;
/*!40000 ALTER TABLE `countries` DISABLE KEYS */;
INSERT INTO `countries` VALUES ('DEU','Alemania',22.00),('ESP','España',15.00),('FRA','Francia',20.00),('GBR','Reino Unido',18.00),('USA','Estados Unidos',24.00);
/*!40000 ALTER TABLE `countries` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employees`
--

DROP TABLE IF EXISTS `employees`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employees` (
  `idEmployee` int NOT NULL AUTO_INCREMENT,
  `empName` varchar(120) NOT NULL,
  `empCountry` char(3) NOT NULL,
  `empIBAN` varchar(34) NOT NULL,
  PRIMARY KEY (`idEmployee`),
  KEY `fk_Employees_Countries1_idx` (`empCountry`),
  CONSTRAINT `fk_Employees_Countries1` FOREIGN KEY (`empCountry`) REFERENCES `countries` (`idCountry`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employees`
--

LOCK TABLES `employees` WRITE;
/*!40000 ALTER TABLE `employees` DISABLE KEYS */;
INSERT INTO `employees` VALUES (1,'Alejandro Martínez Gómez','ESP','ES9121000418450200051331'),(2,'Lucía Fernández Ruiz','ESP','ES6621000418450200051332'),(3,'Carlos Sánchez Ortiz','ESP','ES4121000418450200051333'),(4,'Elena Moreno Serrano','ESP','ES1621000418450200051334'),(5,'David Navarro Molina','ESP','ES8821000418450200051335'),(6,'Jean Dupont Laurent','FRA','FR7630006000011234567890189'),(7,'Camille Bernard Moreau','FRA','FR7630006000011234567890190'),(8,'Pierre Lefebvre Petit','FRA','FR7630006000011234567890191'),(9,'Hans Müller Weber','DEU','DE89370400440532013000'),(10,'Lukas Schmidt Wagner','DEU','DE89370400440532013001'),(11,'Anna Becker Schulz','DEU','DE89370400440532013002'),(12,'James William Smith','GBR','GB29NWBK60161331926819'),(13,'Oliver George Brown','GBR','GB29NWBK60161331926820'),(14,'Sophie Jane Taylor','GBR','GB29NWBK60161331926821'),(15,'Michael John Davis','USA','US64SVBKUS6S3300958879'),(16,'Emily Rose Wilson','USA','US64SVBKUS6S3300958880'),(17,'Daniel Robert Miller','USA','US64SVBKUS6S3300958881'),(18,'Carmen Vega Delgado','ESP','ES3521000418450200051336'),(19,'Antoine Giroud Blanc','FRA','FR7630006000011234567890192'),(20,'Klara Hoffmann Koch','DEU','DE89370400440532013003');
/*!40000 ALTER TABLE `employees` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `monthlypayments`
--

DROP TABLE IF EXISTS `monthlypayments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `monthlypayments` (
  `idMonthlyPayment` int NOT NULL AUTO_INCREMENT,
  `payEmployee` int NOT NULL,
  `payYear` int NOT NULL,
  `payMonth` int NOT NULL,
  `paySalaryGross` decimal(11,2) NOT NULL,
  `paySalaryNet` decimal(11,2) NOT NULL,
  `payBonusApplied` decimal(4,2) NOT NULL,
  PRIMARY KEY (`idMonthlyPayment`),
  KEY `fk_MonthlySalaryPayments_Employees1_idx` (`payEmployee`),
  CONSTRAINT `fk_MonthlySalaryPayments_Employees1` FOREIGN KEY (`payEmployee`) REFERENCES `employees` (`idEmployee`)
) ENGINE=InnoDB AUTO_INCREMENT=121 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `monthlypayments`
--

LOCK TABLES `monthlypayments` WRITE;
/*!40000 ALTER TABLE `monthlypayments` DISABLE KEYS */;
INSERT INTO `monthlypayments` VALUES (1,1,2026,5,4025.00,3421.25,15.00),(2,2,2026,5,4162.50,3538.13,11.00),(3,3,2026,5,4600.00,3910.00,15.00),(4,4,2026,5,3330.00,2830.50,11.00),(5,5,2026,5,4033.33,3428.33,10.00),(6,6,2026,5,4480.00,3584.00,12.00),(7,7,2026,5,4293.33,3434.67,12.00),(8,8,2026,5,3675.00,2940.00,5.00),(9,9,2026,5,5775.83,4505.15,19.50),(10,10,2026,5,5700.00,4446.00,14.00),(11,11,2026,5,4260.00,3322.80,6.50),(12,12,2026,5,5040.00,4132.80,12.00),(13,13,2026,5,3666.67,3006.67,10.00),(14,14,2026,5,4853.33,3979.73,12.00),(15,15,2026,5,8050.00,6118.00,15.00),(16,16,2026,5,7346.67,5583.47,16.00),(17,17,2026,5,6480.00,4924.80,8.00),(18,18,2026,5,3145.00,2673.25,11.00),(19,19,2026,5,4063.33,3250.67,6.00),(20,20,2026,5,5130.00,4001.40,14.00),(21,1,2026,6,4025.00,3421.25,15.00),(22,2,2026,6,4162.50,3538.13,11.00),(23,3,2026,6,4600.00,3910.00,15.00),(24,4,2026,6,3330.00,2830.50,11.00),(25,5,2026,6,4033.33,3428.33,10.00),(26,6,2026,6,4480.00,3584.00,12.00),(27,7,2026,6,4293.33,3434.67,12.00),(28,8,2026,6,3675.00,2940.00,5.00),(29,9,2026,6,5775.83,4505.15,19.50),(30,10,2026,6,5700.00,4446.00,14.00),(31,11,2026,6,4260.00,3322.80,6.50),(32,12,2026,6,5040.00,4132.80,12.00),(33,13,2026,6,3666.67,3006.67,10.00),(34,14,2026,6,4853.33,3979.73,12.00),(35,15,2026,6,8050.00,6118.00,15.00),(36,16,2026,6,7346.67,5583.47,16.00),(37,17,2026,6,6480.00,4924.80,8.00),(38,18,2026,6,3145.00,2673.25,11.00),(39,19,2026,6,4063.33,3250.67,6.00),(40,20,2026,6,5130.00,4001.40,14.00),(41,1,2026,7,4025.00,3421.25,15.00),(42,2,2026,7,4162.50,3538.13,11.00),(43,3,2026,7,4600.00,3910.00,15.00),(44,4,2026,7,3330.00,2830.50,11.00),(45,5,2026,7,4033.33,3428.33,10.00),(46,6,2026,7,4480.00,3584.00,12.00),(47,7,2026,7,4293.33,3434.67,12.00),(48,8,2026,7,3675.00,2940.00,5.00),(49,9,2026,7,5775.83,4505.15,19.50),(50,10,2026,7,5700.00,4446.00,14.00),(51,11,2026,7,4260.00,3322.80,6.50),(52,12,2026,7,5040.00,4132.80,12.00),(53,13,2026,7,3666.67,3006.67,10.00),(54,14,2026,7,4853.33,3979.73,12.00),(55,15,2026,7,8050.00,6118.00,15.00),(56,16,2026,7,7346.67,5583.47,16.00),(57,17,2026,7,6480.00,4924.80,8.00),(58,18,2026,7,3145.00,2673.25,11.00),(59,19,2026,7,4063.33,3250.67,6.00),(60,20,2026,7,5130.00,4001.40,14.00),(61,1,2026,8,4025.00,3421.25,15.00),(62,2,2026,8,4162.50,3538.13,11.00),(63,3,2026,8,4600.00,3910.00,15.00),(64,4,2026,8,3330.00,2830.50,11.00),(65,5,2026,8,4033.33,3428.33,10.00),(66,6,2026,8,4480.00,3584.00,12.00),(67,7,2026,8,4293.33,3434.67,12.00),(68,8,2026,8,3675.00,2940.00,5.00),(69,9,2026,8,5775.83,4505.15,19.50),(70,10,2026,8,5700.00,4446.00,14.00),(71,11,2026,8,4260.00,3322.80,6.50),(72,12,2026,8,5040.00,4132.80,12.00),(73,13,2026,8,3666.67,3006.67,10.00),(74,14,2026,8,4853.33,3979.73,12.00),(75,15,2026,8,8050.00,6118.00,15.00),(76,16,2026,8,7346.67,5583.47,16.00),(77,17,2026,8,6480.00,4924.80,8.00),(78,18,2026,8,3145.00,2673.25,11.00),(79,19,2026,8,4063.33,3250.67,6.00),(80,20,2026,8,5130.00,4001.40,14.00),(81,1,2026,9,4025.00,3421.25,15.00),(82,2,2026,9,4162.50,3538.13,11.00),(83,3,2026,9,4600.00,3910.00,15.00),(84,4,2026,9,3330.00,2830.50,11.00),(85,5,2026,9,4033.33,3428.33,10.00),(86,6,2026,9,4480.00,3584.00,12.00),(87,7,2026,9,4293.33,3434.67,12.00),(88,8,2026,9,3850.00,3080.00,10.00),(89,9,2026,9,5775.83,4505.15,19.50),(90,10,2026,9,5700.00,4446.00,14.00),(91,11,2026,9,4260.00,3322.80,6.50),(92,12,2026,9,5040.00,4132.80,12.00),(93,13,2026,9,3666.67,3006.67,10.00),(94,14,2026,9,4853.33,3979.73,12.00),(95,15,2026,9,8575.00,6517.00,22.50),(96,16,2026,9,7346.67,5583.47,16.00),(97,17,2026,9,6480.00,4924.80,8.00),(98,18,2026,9,3145.00,2673.25,11.00),(99,19,2026,9,4063.33,3250.67,6.00),(100,20,2026,9,5130.00,4001.40,14.00),(101,1,2026,10,4025.00,3421.25,15.00),(102,11,2026,10,4260.00,3322.80,6.50),(103,19,2026,10,4063.33,3250.67,6.00),(104,7,2026,10,4293.33,3434.67,12.00),(105,3,2026,10,4600.00,3910.00,15.00),(106,18,2026,10,3145.00,2673.25,11.00),(107,17,2026,10,6480.00,4924.80,8.00),(108,5,2026,10,4033.33,3428.33,10.00),(109,4,2026,10,3330.00,2830.50,11.00),(110,16,2026,10,7346.67,5583.47,16.00),(111,9,2026,10,5775.83,4505.15,19.50),(112,12,2026,10,5040.00,4132.80,12.00),(113,6,2026,10,4480.00,3584.00,12.00),(114,20,2026,10,5130.00,4001.40,14.00),(115,2,2026,10,4162.50,3538.13,11.00),(116,10,2026,10,5700.00,4446.00,14.00),(117,15,2026,10,8575.00,6517.00,22.50),(118,13,2026,10,3666.67,3006.67,10.00),(119,8,2026,10,3850.00,3080.00,10.00),(120,14,2026,10,4853.33,3979.73,12.00);
/*!40000 ALTER TABLE `monthlypayments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `offices`
--

DROP TABLE IF EXISTS `offices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `offices` (
  `idOffice` int NOT NULL AUTO_INCREMENT,
  `offCountry` char(3) NOT NULL,
  `offCity` varchar(60) NOT NULL,
  `offAddress` varchar(120) NOT NULL,
  `offThreeYearBonus` decimal(4,2) NOT NULL,
  PRIMARY KEY (`idOffice`),
  KEY `fk_Offices_Countries_idx` (`offCountry`),
  CONSTRAINT `fk_Offices_Countries` FOREIGN KEY (`offCountry`) REFERENCES `countries` (`idCountry`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `offices`
--

LOCK TABLES `offices` WRITE;
/*!40000 ALTER TABLE `offices` DISABLE KEYS */;
INSERT INTO `offices` VALUES (1,'ESP','Madrid','Paseo de la Castellana 140',5.00),(2,'ESP','Barcelona','Avinguda Diagonal 640',5.50),(3,'FRA','París','Boulevard Haussmann 82',6.00),(4,'FRA','Lyon','Rue de la République 14',5.00),(5,'DEU','Berlín','Friedrichstraße 180',6.50),(6,'DEU','Múnich','Maximilianstraße 35',7.00),(7,'GBR','Londres','100 Bishopsgate',6.00),(8,'GBR','Mánchester','King Street 45',5.00),(9,'USA','Nueva York','350 5th Avenue (Empire State)',7.50),(10,'USA','San Francisco','500 Howard Street',8.00);
/*!40000 ALTER TABLE `offices` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `positions`
--

DROP TABLE IF EXISTS `positions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `positions` (
  `idPosition` int NOT NULL AUTO_INCREMENT,
  `posEmployee` int NOT NULL,
  `posOffice` int NOT NULL,
  `posName` varchar(100) NOT NULL,
  `posYearSalary` decimal(11,2) NOT NULL,
  `posStartDate` date NOT NULL,
  `posEndDate` date DEFAULT NULL,
  PRIMARY KEY (`idPosition`),
  KEY `fk_Positions_Employees1_idx` (`posEmployee`),
  KEY `fk_Positions_Offices1_idx` (`posOffice`),
  CONSTRAINT `fk_Positions_Employees1` FOREIGN KEY (`posEmployee`) REFERENCES `employees` (`idEmployee`),
  CONSTRAINT `fk_Positions_Offices1` FOREIGN KEY (`posOffice`) REFERENCES `offices` (`idOffice`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `positions`
--

LOCK TABLES `positions` WRITE;
/*!40000 ALTER TABLE `positions` DISABLE KEYS */;
INSERT INTO `positions` VALUES (1,1,1,'Programador Junior',24000.00,'2017-01-15','2020-03-31'),(2,1,1,'Ingeniero de Software Senior',42000.00,'2020-04-01',NULL),(3,2,2,'Especialista en Marketing',28000.00,'2018-05-01','2021-06-30'),(4,2,2,'Directora de Marketing',45000.00,'2021-07-01',NULL),(5,3,1,'Técnico de Soporte',22000.00,'2016-09-01','2019-08-31'),(6,3,1,'DevOps Lead',48000.00,'2019-09-01',NULL),(7,4,2,'Contable Senior',36000.00,'2019-02-01',NULL),(8,5,1,'Analista de Datos Junior',26000.00,'2020-01-10','2022-06-15'),(9,5,1,'Data Scientist Senior',44000.00,'2022-06-16',NULL),(10,6,3,'Chef de Projet IT',48000.00,'2018-03-01',NULL),(11,7,4,'Consultante RH Junior',27000.00,'2019-06-01','2022-05-31'),(12,7,3,'Responsable RH France',46000.00,'2022-06-01',NULL),(13,8,4,'Ingénieur Sécurité',42000.00,'2020-09-15',NULL),(14,9,5,'Softwareentwickler',38000.00,'2017-04-01','2021-03-31'),(15,9,5,'Software Architekt',58000.00,'2021-04-01',NULL),(16,10,6,'Cloud Architekt',60000.00,'2019-11-01',NULL),(17,11,5,'Produktmanagerin',48000.00,'2021-01-15',NULL),(18,12,7,'Junior Financial Analyst',32000.00,'2018-08-01','2021-09-30'),(19,12,7,'Senior Financial Controller',54000.00,'2021-10-01',NULL),(20,13,8,'Operations Manager',40000.00,'2020-02-01',NULL),(21,14,7,'Legal Counsel',52000.00,'2019-07-01',NULL),(22,15,9,'QA Engineer',45000.00,'2017-10-01','2021-05-31'),(23,15,9,'VP of Quality Assurance',84000.00,'2021-06-01',NULL),(24,16,10,'Full Stack Engineer',76000.00,'2020-04-01',NULL),(25,17,10,'Product Designer Lead',72000.00,'2021-03-01',NULL),(26,18,2,'Diseñadora Gráfica Junior',22000.00,'2020-03-01','2022-07-31'),(27,18,2,'Diseñadora UX/UI Senior',34000.00,'2022-08-01',NULL),(28,19,3,'Support Technique',26000.00,'2021-01-10','2023-04-30'),(29,19,3,'Analyste Cybersécurité',46000.00,'2023-05-01',NULL),(30,20,6,'Lead Database Administrator',54000.00,'2019-05-01',NULL);
/*!40000 ALTER TABLE `positions` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-09 22:41:34
