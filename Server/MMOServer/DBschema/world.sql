-- phpMyAdmin SQL Dump
-- version 4.5.4.1deb2ubuntu2
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Jan 26, 2018 at 07:58 PM
-- Server version: 5.7.21-0ubuntu0.16.04.1
-- PHP Version: 7.0.22-0ubuntu0.16.04.1

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `world`
--

-- --------------------------------------------------------

--
-- Table structure for table `character_positions`
--

CREATE TABLE `character_positions` (
  `characterId` int(11) NOT NULL,
  `xPos` int(10) NOT NULL DEFAULT '0',
  `yPos` int(10) NOT NULL DEFAULT '0',
  `zone` varchar(20) NOT NULL DEFAULT 'test'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `online_players`
--

CREATE TABLE `online_players` (
  `sessionId` int(90) NOT NULL,
  `charId` int(11) NOT NULL,
  `accountId` int(11) NOT NULL,
  `name` varchar(10) NOT NULL,
  `ipAddress` varchar(90) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `character_positions`
--
ALTER TABLE `character_positions`
  ADD PRIMARY KEY (`characterId`);

--
-- Indexes for table `online_players`
--
ALTER TABLE `online_players`
  ADD PRIMARY KEY (`sessionId`),
  ADD KEY `charId` (`charId`),
  ADD KEY `accountId` (`accountId`),
  ADD KEY `name` (`name`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `online_players`
--
ALTER TABLE `online_players`
  MODIFY `sessionId` int(90) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;
--
-- Constraints for dumped tables
--

--
-- Constraints for table `character_positions`
--
ALTER TABLE `character_positions`
  ADD CONSTRAINT `character_positions_ibfk_1` FOREIGN KEY (`characterId`) REFERENCES `login`.`characters` (`id`);

--
-- Constraints for table `online_players`
--
ALTER TABLE `online_players`
  ADD CONSTRAINT `online_players_ibfk_1` FOREIGN KEY (`charId`) REFERENCES `login`.`characters` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `online_players_ibfk_2` FOREIGN KEY (`accountId`) REFERENCES `login`.`account` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `online_players_ibfk_3` FOREIGN KEY (`name`) REFERENCES `login`.`characters` (`name`) ON DELETE NO ACTION ON UPDATE NO ACTION;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
