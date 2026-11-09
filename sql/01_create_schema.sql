-- =============================================================================
-- SCRIPT DE CREACIÓN DE BASE DE DATOS: company
-- Asignatura: Bases de Datos - Grado en Ingeniería Informática
-- Proyecto: Aplicación de Nóminas (Laboratorio 2025/2026)
-- Esquema: idéntico al modelo oficial facilitado por el profesor (company.mwb).
--          No se añaden restricciones (UNIQUE, CHECK...) que no estén en el modelo.
-- =============================================================================

DROP DATABASE IF EXISTS company;
CREATE DATABASE company
  DEFAULT CHARACTER SET utf8
  COLLATE utf8_general_ci;

USE company;

-- -----------------------------------------------------------------------------
-- 1. TABLA: Countries
-- -----------------------------------------------------------------------------
CREATE TABLE Countries (
    idCountry   CHAR(3)        NOT NULL,
    couName     VARCHAR(120)   NOT NULL,
    couTaxBase  DECIMAL(4,2)   NOT NULL,
    PRIMARY KEY (idCountry)
) ENGINE=InnoDB;

-- -----------------------------------------------------------------------------
-- 2. TABLA: Offices
-- -----------------------------------------------------------------------------
CREATE TABLE Offices (
    idOffice           INT            NOT NULL AUTO_INCREMENT,
    offCountry         CHAR(3)        NOT NULL,
    offCity            VARCHAR(60)    NOT NULL,
    offAddress         VARCHAR(120)   NOT NULL,
    offThreeYearBonus  DECIMAL(4,2)   NOT NULL,
    PRIMARY KEY (idOffice),
    INDEX fk_Offices_Countries_idx (offCountry ASC),
    CONSTRAINT fk_Offices_Countries FOREIGN KEY (offCountry)
        REFERENCES Countries (idCountry)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
) ENGINE=InnoDB;

-- -----------------------------------------------------------------------------
-- 3. TABLA: Employees
-- -----------------------------------------------------------------------------
CREATE TABLE Employees (
    idEmployee  INT            NOT NULL AUTO_INCREMENT,
    empName     VARCHAR(120)   NOT NULL,
    empCountry  CHAR(3)        NOT NULL,
    empIBAN     VARCHAR(34)    NOT NULL,
    PRIMARY KEY (idEmployee),
    INDEX fk_Employees_Countries1_idx (empCountry ASC),
    CONSTRAINT fk_Employees_Countries1 FOREIGN KEY (empCountry)
        REFERENCES Countries (idCountry)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
) ENGINE=InnoDB;

-- -----------------------------------------------------------------------------
-- 4. TABLA: Positions
-- -----------------------------------------------------------------------------
CREATE TABLE Positions (
    idPosition     INT             NOT NULL AUTO_INCREMENT,
    posEmployee    INT             NOT NULL,
    posOffice      INT             NOT NULL,
    posName        VARCHAR(100)    NOT NULL,
    posYearSalary  DECIMAL(11,2)   NOT NULL,
    posStartDate   DATE            NOT NULL,
    posEndDate     DATE            NULL,
    PRIMARY KEY (idPosition),
    INDEX fk_Positions_Employees1_idx (posEmployee ASC),
    INDEX fk_Positions_Offices1_idx (posOffice ASC),
    CONSTRAINT fk_Positions_Employees1 FOREIGN KEY (posEmployee)
        REFERENCES Employees (idEmployee)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT fk_Positions_Offices1 FOREIGN KEY (posOffice)
        REFERENCES Offices (idOffice)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
) ENGINE=InnoDB;

-- -----------------------------------------------------------------------------
-- 5. TABLA: MonthlyPayments
-- -----------------------------------------------------------------------------
CREATE TABLE MonthlyPayments (
    idMonthlyPayment  INT             NOT NULL AUTO_INCREMENT,
    payEmployee       INT             NOT NULL,
    payYear           INT             NOT NULL,
    payMonth          INT             NOT NULL,
    paySalaryGross    DECIMAL(11,2)   NOT NULL,
    paySalaryNet      DECIMAL(11,2)   NOT NULL,
    payBonusApplied   DECIMAL(4,2)    NOT NULL,
    PRIMARY KEY (idMonthlyPayment),
    INDEX fk_MonthlySalaryPayments_Employees1_idx (payEmployee ASC),
    CONSTRAINT fk_MonthlySalaryPayments_Employees1 FOREIGN KEY (payEmployee)
        REFERENCES Employees (idEmployee)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
) ENGINE=InnoDB;
