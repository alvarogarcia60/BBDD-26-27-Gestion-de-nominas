-- =============================================================================
-- SCRIPT DE CREACIÓN DE BASE DE DATOS: nominas_db
-- Asignatura: Bases de Datos - Grado en Ingeniería Informática
-- Proyecto: Aplicación de Nóminas (Laboratorio 2025/2026)
-- Motor: MySQL 8.0+ / MariaDB 10.5+ (Motor de almacenamiento InnoDB)
-- Codificación: UTF-8 (utf8mb4_unicode_ci)
-- =============================================================================

DROP DATABASE IF EXISTS nominas_db;
CREATE DATABASE nominas_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE nominas_db;

-- -----------------------------------------------------------------------------
-- 1. TABLA: Countries
-- Almacena los países donde la empresa tiene presencia u origen de empleados.
-- idCountry: Código de 3 caracteres alfabéticos (ej. ESP, FRA).
-- couTaxBase: Porcentaje mensual de impuestos aplicable en el país.
-- -----------------------------------------------------------------------------
CREATE TABLE Countries (
    idCountry   CHAR(3)        NOT NULL,
    couName     VARCHAR(120)   NOT NULL,
    couTaxBase  DECIMAL(4,2)   NOT NULL,
    CONSTRAINT pk_countries PRIMARY KEY (idCountry),
    CONSTRAINT chk_couTaxBase CHECK (couTaxBase >= 0.00 AND couTaxBase <= 100.00)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 2. TABLA: Offices
-- Almacena las sedes y oficinas físicas de la empresa en distintas ciudades.
-- offThreeYearBonus: Porcentaje base de bonus por trienio aplicable en la sede.
-- -----------------------------------------------------------------------------
CREATE TABLE Offices (
    idOffice           INT AUTO_INCREMENT NOT NULL,
    offCountry         CHAR(3)            NOT NULL,
    offCity            VARCHAR(60)        NOT NULL,
    offAddress         VARCHAR(120)       NOT NULL,
    offThreeYearBonus  DECIMAL(4,2)       NOT NULL,
    CONSTRAINT pk_offices PRIMARY KEY (idOffice),
    CONSTRAINT fk_offices_countries FOREIGN KEY (offCountry) 
        REFERENCES Countries (idCountry) 
        ON UPDATE CASCADE 
        ON DELETE RESTRICT,
    CONSTRAINT chk_offThreeYearBonus CHECK (offThreeYearBonus >= 0.00 AND offThreeYearBonus <= 100.00)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX idx_offices_country ON Offices (offCountry);

-- -----------------------------------------------------------------------------
-- 3. TABLA: Employees
-- Almacena la información de los trabajadores de la multinacional.
-- empCountry: País de residencia u origen del trabajador.
-- empIBAN: Cuenta bancaria para el abono de la nómina (única por empleado).
-- -----------------------------------------------------------------------------
CREATE TABLE Employees (
    idEmployee  INT AUTO_INCREMENT NOT NULL,
    empName     VARCHAR(120)       NOT NULL,
    empCountry  CHAR(3)            NOT NULL,
    empIBAN     VARCHAR(34)        NOT NULL,
    CONSTRAINT pk_employees PRIMARY KEY (idEmployee),
    CONSTRAINT fk_employees_countries FOREIGN KEY (empCountry) 
        REFERENCES Countries (idCountry) 
        ON UPDATE CASCADE 
        ON DELETE RESTRICT,
    CONSTRAINT uq_employees_iban UNIQUE KEY (empIBAN)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX idx_employees_country ON Employees (empCountry);

-- -----------------------------------------------------------------------------
-- 4. TABLA: Positions
-- Representa los contratos / puestos asignados a cada empleado a lo largo del tiempo.
-- posEndDate: Si es NULL, indica que el empleado ocupa actualmente dicho puesto.
-- Si tiene valor, debe ser en el pasado y no solaparse con otros periodos del mismo empleado.
-- -----------------------------------------------------------------------------
CREATE TABLE Positions (
    idPosition     INT AUTO_INCREMENT NOT NULL,
    posEmployee    INT                NOT NULL,
    posOffice      INT                NOT NULL,
    posName        VARCHAR(100)       NOT NULL,
    posYearSalary  DECIMAL(11,2)      NOT NULL,
    posStartDate   DATE               NOT NULL,
    posEndDate     DATE               NULL,
    CONSTRAINT pk_positions PRIMARY KEY (idPosition),
    CONSTRAINT fk_positions_employees FOREIGN KEY (posEmployee) 
        REFERENCES Employees (idEmployee) 
        ON UPDATE CASCADE 
        ON DELETE CASCADE,
    CONSTRAINT fk_positions_offices FOREIGN KEY (posOffice) 
        REFERENCES Offices (idOffice) 
        ON UPDATE CASCADE 
        ON DELETE RESTRICT,
    CONSTRAINT chk_positions_salary CHECK (posYearSalary >= 0.00),
    CONSTRAINT chk_positions_dates CHECK (posEndDate IS NULL OR posEndDate >= posStartDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX idx_positions_employee ON Positions (posEmployee);
CREATE INDEX idx_positions_office ON Positions (posOffice);
CREATE INDEX idx_positions_dates ON Positions (posStartDate, posEndDate);

-- -----------------------------------------------------------------------------
-- 5. TABLA: MonthlyPayments
-- Almacena las nóminas mensuales devengadas y abonadas a los trabajadores.
-- paySalaryGross: Salario mensual bruto (proporcional + bonus por trienios).
-- paySalaryNet: Salario neto mensual (bruto menos retención del país de la oficina).
-- payBonusApplied: Porcentaje de bonus aplicado en la nómina.
-- Restricción UNIQUE en (payEmployee, payYear, payMonth) para impedir pagos duplicados.
-- -----------------------------------------------------------------------------
CREATE TABLE MonthlyPayments (
    idMonthlyPayment  INT AUTO_INCREMENT NOT NULL,
    payEmployee       INT                NOT NULL,
    payYear           INT                NOT NULL,
    payMonth          INT                NOT NULL,
    paySalaryGross    DECIMAL(11,2)      NOT NULL,
    paySalaryNet      DECIMAL(11,2)      NOT NULL,
    payBonusApplied   DECIMAL(5,2)       NOT NULL,  -- 5,2: varios trienios pueden superar el 99.99 %
    CONSTRAINT pk_monthlypayments PRIMARY KEY (idMonthlyPayment),
    CONSTRAINT fk_monthlypayments_employees FOREIGN KEY (payEmployee) 
        REFERENCES Employees (idEmployee) 
        ON UPDATE CASCADE 
        ON DELETE CASCADE,
    CONSTRAINT uq_monthlypayments_period UNIQUE KEY (payEmployee, payYear, payMonth),
    CONSTRAINT chk_monthlypayments_month CHECK (payMonth BETWEEN 1 AND 12),
    CONSTRAINT chk_monthlypayments_gross CHECK (paySalaryGross >= 0.00),
    CONSTRAINT chk_monthlypayments_net CHECK (paySalaryNet >= 0.00),
    CONSTRAINT chk_monthlypayments_bonus CHECK (payBonusApplied >= 0.00)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX idx_monthlypayments_period ON MonthlyPayments (payYear, payMonth);
CREATE INDEX idx_monthlypayments_employee ON MonthlyPayments (payEmployee);
