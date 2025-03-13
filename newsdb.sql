-- phpMyAdmin SQL Dump
-- version 3.3.9.2
-- http://www.phpmyadmin.net
--
-- Host: 127.0.0.1
-- Generation Time: Mar 13, 2025 at 04:31 AM
-- Server version: 5.5.10
-- PHP Version: 5.3.6

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Create database if not exists and use it
--

CREATE DATABASE IF NOT EXISTS `newsdb`;
USE `newsdb`;

--
-- Table structure for table `favorites`
--

CREATE TABLE IF NOT EXISTS `favorites` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `url` text NOT NULL,
  `description` text,
  `image_url` text,
  `published_at` datetime DEFAULT NULL,
  `added_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `category` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=14 ;

--
-- Dumping data for table `favorites`
--

INSERT INTO `favorites` (`id`, `title`, `url`, `description`, `image_url`, `published_at`, `added_at`, `category`) VALUES
(11, 'Watch These Palantir Stock Price Levels After Recent Sell-Off - Investopedia', 'https://www.investopedia.com/watch-these-palantir-stock-price-levels-after-recent-sell-off-pltr-11694528', 'Palantir shares gained ground on Tuesday as the stock took a breather from a three-week slump that has seen the AI investor favorite lose more than a third of its market value. Here are the key chart levels to monitor.', 'https://www.investopedia.com/thmb/41l0pSWtL4hwwPVSa--bzXufw90=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/PLTRChart-b51ddc667e844313b43db0bfae4dc965.gif', '2025-03-11 22:28:13', '2025-03-13 02:25:56', 'Business'),
(12, 'KaVontae Turpin ''blessed'' to land Cowboys'' extension, hungry for more: ''I''m a weapon'' - DallasCowboys.com', 'https://www.dallascowboys.com/news/kavontae-turpin-blessed-to-land-cowboys-extension-hungry-for-more-i-m-a-weapon', 'Having already overcome a mountain of challenges to become one of the best players in the NFL, KaVontae Turpin is still starving to eat more for the Dallas Cowboys.', 'https://static.clubs.nfl.com/image/upload/t_editorial_landscape_12_desktop/cowboys/dawsvrshec2soekxxneg', '2025-03-11 20:36:47', '2025-03-13 03:32:56', 'Sports');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE IF NOT EXISTS `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `password_hash` varchar(256) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=2 ;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `username`, `password_hash`) VALUES
(1, 'test', '9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08');
