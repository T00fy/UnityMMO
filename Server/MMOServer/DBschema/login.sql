-- phpMyAdmin SQL Dump
-- version 4.7.4
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1:3306
-- Generation Time: Jan 01, 2018 at 08:48 PM
-- Server version: 5.7.19
-- PHP Version: 5.6.31

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `login`
--

-- --------------------------------------------------------

--
-- Table structure for table `account`
--

DROP TABLE IF EXISTS `account`;
CREATE TABLE IF NOT EXISTS `account` (
  `id` int(7) NOT NULL AUTO_INCREMENT,
  `username` varchar(12) NOT NULL,
  `password` varchar(12) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=latin1;

--
-- Dumping data for table `account`
--

INSERT INTO `account` (`id`, `username`, `password`) VALUES
(2, 'blah', 'test'),
(3, 'account2', 'test');

-- --------------------------------------------------------

--
-- Table structure for table `characters`
--

DROP TABLE IF EXISTS `characters`;
CREATE TABLE IF NOT EXISTS `characters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `characterSlot` int(2) NOT NULL,
  `accountId` int(7) NOT NULL,
  `name` varchar(10) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `AccountID` (`accountId`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;

--
-- Dumping data for table `characters`
--

INSERT INTO `characters` (`id`, `characterSlot`, `accountId`, `name`) VALUES
(1, 1, 2, 'testguy'),
(2, 2, 2, 'testguyy'),
(3, 1, 3, 'blahh'),
(4, 2, 3, 'sqltest');

-- --------------------------------------------------------

--
-- Table structure for table `character_info`
--

DROP TABLE IF EXISTS `character_info`;
CREATE TABLE IF NOT EXISTS `character_info` (
  `charId` int(8) NOT NULL,
  `strength` int(3) NOT NULL,
  `agility` int(3) NOT NULL,
  `intellect` int(3) NOT NULL,
  `vitality` int(3) NOT NULL,
  `dexterity` int(3) NOT NULL,
  KEY `charId` (`charId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `character_info`
--

INSERT INTO `character_info` (`charId`, `strength`, `agility`, `intellect`, `vitality`, `dexterity`) VALUES
(1, 9, 9, 9, 9, 9),
(2, 12, 9, 9, 9, 9),
(3, 9, 9, 9, 9, 9),
(4, 9, 9, 9, 9, 9);

--
-- Constraints for dumped tables
--

--
-- Constraints for table `characters`
--
ALTER TABLE `characters`
  ADD CONSTRAINT `characters_ibfk_2` FOREIGN KEY (`accountId`) REFERENCES `account` (`id`);

--
-- Constraints for table `character_info`
--
ALTER TABLE `character_info`
  ADD CONSTRAINT `character_info_ibfk_1` FOREIGN KEY (`charId`) REFERENCES `characters` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
