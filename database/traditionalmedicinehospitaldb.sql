USE [master]
GO
/****** Object:  Database [TraditionalMedicineHospitalDB]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE DATABASE [TraditionalMedicineHospitalDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'TraditionalMedicineHospitalDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TraditionalMedicineHospitalDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'TraditionalMedicineHospitalDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TraditionalMedicineHospitalDB_log.ldf' , SIZE = 139264KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [TraditionalMedicineHospitalDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET RECOVERY FULL 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET  MULTI_USER 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'TraditionalMedicineHospitalDB', N'ON'
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET QUERY_STORE = OFF
GO
USE [TraditionalMedicineHospitalDB]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Assignment]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Assignment](
	[Id] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
	[Note] [nvarchar](max) NULL,
	[EmployeeId] [uniqueidentifier] NOT NULL,
	[TreatmentRecordId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Code] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_Assignment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Department]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Department](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Employee]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employee](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Gender] [int] NOT NULL,
	[DateOfBirth] [datetime2](7) NOT NULL,
	[IdentityNumber] [nvarchar](12) NOT NULL,
	[EmailAddress] [nvarchar](max) NOT NULL,
	[PhoneNumber] [nvarchar](max) NOT NULL,
	[Images] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[EmployeeCategoryId] [uniqueidentifier] NOT NULL,
	[Address] [nvarchar](500) NOT NULL,
	[RoomId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeCategory]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeCategory](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_EmployeeCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HealthInsurance]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HealthInsurance](
	[Id] [uniqueidentifier] NOT NULL,
	[Number] [nvarchar](max) NOT NULL,
	[ExpiryDate] [datetime2](7) NOT NULL,
	[PlaceOfRegistration] [int] NOT NULL,
	[PatientId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Code] [nvarchar](10) NOT NULL,
	[IsRightRoute] [bit] NOT NULL,
 CONSTRAINT [PK_HealthInsurance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Medicine]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Medicine](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Images] [nvarchar](max) NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[StockQuantity] [int] NOT NULL,
	[StockUnit] [int] NOT NULL,
	[Manufacturer] [int] NOT NULL,
	[ManufacturedDate] [datetime2](7) NOT NULL,
	[ActiveIngredient] [nvarchar](max) NOT NULL,
	[ExpiryDate] [datetime2](7) NOT NULL,
	[MedicineCategoryId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Medicine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MedicineCategory]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicineCategory](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_MedicineCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Patient]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patient](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[IdentityNumber] [nvarchar](12) NULL,
	[DateOfBirth] [datetime2](7) NOT NULL,
	[Gender] [int] NOT NULL,
	[Address] [nvarchar](500) NOT NULL,
	[PhoneNumber] [nvarchar](max) NOT NULL,
	[EmailAddress] [nvarchar](max) NULL,
	[Images] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Patient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[Id] [uniqueidentifier] NOT NULL,
	[PaymentDate] [datetime2](7) NOT NULL,
	[Note] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[TreatmentRecordId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Type] [int] NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Prescription]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Prescription](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[PrescriptionDate] [datetime2](7) NOT NULL,
	[Note] [nvarchar](max) NULL,
	[TreatmentRecordId] [uniqueidentifier] NOT NULL,
	[EmployeeId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Prescription] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PrescriptionDetail]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrescriptionDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[Quantity] [int] NOT NULL,
	[PrescriptionId] [uniqueidentifier] NOT NULL,
	[MedicineId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_PrescriptionDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Regulation]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Regulation](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[EffectiveDate] [datetime2](7) NOT NULL,
	[ExpirationDate] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Regulation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Room]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Room](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[TreatmentMethodId] [uniqueidentifier] NULL,
	[DepartmentId] [uniqueidentifier] NOT NULL,
	[RoomType] [int] NOT NULL,
 CONSTRAINT [PK_Room] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TreatmentMethod]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TreatmentMethod](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Cost] [decimal](18, 2) NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DepartmentId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_TreatmentMethod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TreatmentRecord]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TreatmentRecord](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[Diagnosis] [int] NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
	[Note] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[PatientId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[SuspendedBy] [nvarchar](max) NULL,
	[SuspendedDate] [datetime2](7) NULL,
	[SuspendedNote] [nvarchar](max) NULL,
	[SuspendedReason] [nvarchar](max) NULL,
	[IsViolated] [bit] NOT NULL,
	[AdvancePayment] [decimal](18, 2) NULL,
 CONSTRAINT [PK_TreatmentRecord] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TreatmentRecord_Regulation]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TreatmentRecord_Regulation](
	[Id] [uniqueidentifier] NOT NULL,
	[ExecutionDate] [datetime2](7) NOT NULL,
	[Note] [nvarchar](max) NULL,
	[TreatmentRecordId] [uniqueidentifier] NOT NULL,
	[RegulationId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Code] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_TreatmentRecord_Regulation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TreatmentRecordDetail]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TreatmentRecordDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[Note] [nvarchar](max) NULL,
	[TreatmentRecordId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Code] [nvarchar](10) NOT NULL,
	[RoomId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_TreatmentRecordDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TreatmentTracking]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TreatmentTracking](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[TrackingDate] [datetime2](7) NOT NULL,
	[Note] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Status] [int] NOT NULL,
	[TreatmentRecordDetailId] [uniqueidentifier] NULL,
	[EmployeeId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_TreatmentTracking] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [uniqueidentifier] NOT NULL,
	[Username] [nvarchar](20) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[Role] [int] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[EmployeeId] [uniqueidentifier] NULL,
	[IsFirstLogin] [bit] NOT NULL,
	[PatientId] [uniqueidentifier] NULL,
	[UsedResetCode] [nvarchar](max) NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WarningSent]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WarningSent](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
	[PatientId] [uniqueidentifier] NOT NULL,
	[TreatmentRecordDetailId] [uniqueidentifier] NOT NULL,
	[FirstAbsenceDate] [datetime2](7) NOT NULL,
	[SentAt] [datetime2](7) NOT NULL,
	[Type] [int] NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](max) NULL,
	[UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_WarningSent] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_Assignment_EmployeeId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Assignment_EmployeeId] ON [dbo].[Assignment]
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Assignment_TreatmentRecordId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Assignment_TreatmentRecordId] ON [dbo].[Assignment]
(
	[TreatmentRecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Employee_EmployeeCategoryId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Employee_EmployeeCategoryId] ON [dbo].[Employee]
(
	[EmployeeCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Employee_RoomId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Employee_RoomId] ON [dbo].[Employee]
(
	[RoomId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HealthInsurance_PatientId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_HealthInsurance_PatientId] ON [dbo].[HealthInsurance]
(
	[PatientId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Medicine_MedicineCategoryId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Medicine_MedicineCategoryId] ON [dbo].[Medicine]
(
	[MedicineCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_TreatmentRecordId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Payment_TreatmentRecordId] ON [dbo].[Payment]
(
	[TreatmentRecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Prescription_EmployeeId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Prescription_EmployeeId] ON [dbo].[Prescription]
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Prescription_TreatmentRecordId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Prescription_TreatmentRecordId] ON [dbo].[Prescription]
(
	[TreatmentRecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PrescriptionDetail_MedicineId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_PrescriptionDetail_MedicineId] ON [dbo].[PrescriptionDetail]
(
	[MedicineId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PrescriptionDetail_PrescriptionId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_PrescriptionDetail_PrescriptionId] ON [dbo].[PrescriptionDetail]
(
	[PrescriptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Room_DepartmentId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Room_DepartmentId] ON [dbo].[Room]
(
	[DepartmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Room_TreatmentMethodId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_Room_TreatmentMethodId] ON [dbo].[Room]
(
	[TreatmentMethodId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TreatmentRecord_PatientId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_TreatmentRecord_PatientId] ON [dbo].[TreatmentRecord]
(
	[PatientId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TreatmentRecord_Regulation_RegulationId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_TreatmentRecord_Regulation_RegulationId] ON [dbo].[TreatmentRecord_Regulation]
(
	[RegulationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TreatmentRecord_Regulation_TreatmentRecordId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_TreatmentRecord_Regulation_TreatmentRecordId] ON [dbo].[TreatmentRecord_Regulation]
(
	[TreatmentRecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TreatmentRecordDetail_RoomId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_TreatmentRecordDetail_RoomId] ON [dbo].[TreatmentRecordDetail]
(
	[RoomId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TreatmentRecordDetail_TreatmentRecordId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_TreatmentRecordDetail_TreatmentRecordId] ON [dbo].[TreatmentRecordDetail]
(
	[TreatmentRecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TreatmentTracking_TreatmentRecordDetailId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE NONCLUSTERED INDEX [IX_TreatmentTracking_TreatmentRecordDetailId] ON [dbo].[TreatmentTracking]
(
	[TreatmentRecordDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_User_EmployeeId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_EmployeeId] ON [dbo].[User]
(
	[EmployeeId] ASC
)
WHERE ([EmployeeId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_User_PatientId]    Script Date: Thứ Hai 16 06 2025 1:00:48 CH ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_PatientId] ON [dbo].[User]
(
	[PatientId] ASC
)
WHERE ([PatientId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Assignment] ADD  DEFAULT (N'') FOR [Code]
GO
ALTER TABLE [dbo].[Employee] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [EmployeeCategoryId]
GO
ALTER TABLE [dbo].[Employee] ADD  DEFAULT (N'') FOR [Address]
GO
ALTER TABLE [dbo].[Employee] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [RoomId]
GO
ALTER TABLE [dbo].[HealthInsurance] ADD  DEFAULT (N'') FOR [Code]
GO
ALTER TABLE [dbo].[HealthInsurance] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsRightRoute]
GO
ALTER TABLE [dbo].[Medicine] ADD  CONSTRAINT [DF__Medicine__Create__05D8E0BE]  DEFAULT (N'') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[MedicineCategory] ADD  DEFAULT (N'') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[Payment] ADD  DEFAULT (N'') FOR [Code]
GO
ALTER TABLE [dbo].[Room] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [DepartmentId]
GO
ALTER TABLE [dbo].[Room] ADD  DEFAULT ((0)) FOR [RoomType]
GO
ALTER TABLE [dbo].[TreatmentMethod] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [DepartmentId]
GO
ALTER TABLE [dbo].[TreatmentRecord] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsViolated]
GO
ALTER TABLE [dbo].[TreatmentRecord_Regulation] ADD  DEFAULT (N'') FOR [Code]
GO
ALTER TABLE [dbo].[TreatmentRecordDetail] ADD  DEFAULT (N'') FOR [Code]
GO
ALTER TABLE [dbo].[TreatmentRecordDetail] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [RoomId]
GO
ALTER TABLE [dbo].[TreatmentTracking] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[TreatmentTracking] ADD  CONSTRAINT [DF__Treatment__Statu__7908F585]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsFirstLogin]
GO
ALTER TABLE [dbo].[Assignment]  WITH CHECK ADD  CONSTRAINT [FK_Assignment_Employee_EmployeeId] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Assignment] CHECK CONSTRAINT [FK_Assignment_Employee_EmployeeId]
GO
ALTER TABLE [dbo].[Assignment]  WITH CHECK ADD  CONSTRAINT [FK_Assignment_TreatmentRecord_TreatmentRecordId] FOREIGN KEY([TreatmentRecordId])
REFERENCES [dbo].[TreatmentRecord] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Assignment] CHECK CONSTRAINT [FK_Assignment_TreatmentRecord_TreatmentRecordId]
GO
ALTER TABLE [dbo].[Employee]  WITH CHECK ADD  CONSTRAINT [FK_Employee_EmployeeCategory_EmployeeCategoryId] FOREIGN KEY([EmployeeCategoryId])
REFERENCES [dbo].[EmployeeCategory] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Employee] CHECK CONSTRAINT [FK_Employee_EmployeeCategory_EmployeeCategoryId]
GO
ALTER TABLE [dbo].[Employee]  WITH CHECK ADD  CONSTRAINT [FK_Employee_Room_RoomId] FOREIGN KEY([RoomId])
REFERENCES [dbo].[Room] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Employee] CHECK CONSTRAINT [FK_Employee_Room_RoomId]
GO
ALTER TABLE [dbo].[HealthInsurance]  WITH CHECK ADD  CONSTRAINT [FK_HealthInsurance_Patient_PatientId] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[HealthInsurance] CHECK CONSTRAINT [FK_HealthInsurance_Patient_PatientId]
GO
ALTER TABLE [dbo].[Medicine]  WITH CHECK ADD  CONSTRAINT [FK_Medicine_MedicineCategory_MedicineCategoryId] FOREIGN KEY([MedicineCategoryId])
REFERENCES [dbo].[MedicineCategory] ([Id])
GO
ALTER TABLE [dbo].[Medicine] CHECK CONSTRAINT [FK_Medicine_MedicineCategory_MedicineCategoryId]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_Payment_TreatmentRecord_TreatmentRecordId] FOREIGN KEY([TreatmentRecordId])
REFERENCES [dbo].[TreatmentRecord] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK_Payment_TreatmentRecord_TreatmentRecordId]
GO
ALTER TABLE [dbo].[Prescription]  WITH CHECK ADD  CONSTRAINT [FK_Prescription_Employee_EmployeeId] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Prescription] CHECK CONSTRAINT [FK_Prescription_Employee_EmployeeId]
GO
ALTER TABLE [dbo].[Prescription]  WITH CHECK ADD  CONSTRAINT [FK_Prescription_TreatmentRecord_TreatmentRecordId] FOREIGN KEY([TreatmentRecordId])
REFERENCES [dbo].[TreatmentRecord] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Prescription] CHECK CONSTRAINT [FK_Prescription_TreatmentRecord_TreatmentRecordId]
GO
ALTER TABLE [dbo].[PrescriptionDetail]  WITH CHECK ADD  CONSTRAINT [FK_PrescriptionDetail_Medicine_MedicineId] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[Medicine] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PrescriptionDetail] CHECK CONSTRAINT [FK_PrescriptionDetail_Medicine_MedicineId]
GO
ALTER TABLE [dbo].[PrescriptionDetail]  WITH CHECK ADD  CONSTRAINT [FK_PrescriptionDetail_Prescription_PrescriptionId] FOREIGN KEY([PrescriptionId])
REFERENCES [dbo].[Prescription] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PrescriptionDetail] CHECK CONSTRAINT [FK_PrescriptionDetail_Prescription_PrescriptionId]
GO
ALTER TABLE [dbo].[Room]  WITH CHECK ADD  CONSTRAINT [FK_Room_Department_DepartmentId] FOREIGN KEY([DepartmentId])
REFERENCES [dbo].[Department] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Room] CHECK CONSTRAINT [FK_Room_Department_DepartmentId]
GO
ALTER TABLE [dbo].[Room]  WITH CHECK ADD  CONSTRAINT [FK_Room_TreatmentMethod_TreatmentMethodId] FOREIGN KEY([TreatmentMethodId])
REFERENCES [dbo].[TreatmentMethod] ([Id])
GO
ALTER TABLE [dbo].[Room] CHECK CONSTRAINT [FK_Room_TreatmentMethod_TreatmentMethodId]
GO
ALTER TABLE [dbo].[TreatmentRecord]  WITH CHECK ADD  CONSTRAINT [FK_TreatmentRecord_Patient_PatientId] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[TreatmentRecord] CHECK CONSTRAINT [FK_TreatmentRecord_Patient_PatientId]
GO
ALTER TABLE [dbo].[TreatmentRecord_Regulation]  WITH CHECK ADD  CONSTRAINT [FK_TreatmentRecord_Regulation_Regulation_RegulationId] FOREIGN KEY([RegulationId])
REFERENCES [dbo].[Regulation] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TreatmentRecord_Regulation] CHECK CONSTRAINT [FK_TreatmentRecord_Regulation_Regulation_RegulationId]
GO
ALTER TABLE [dbo].[TreatmentRecord_Regulation]  WITH CHECK ADD  CONSTRAINT [FK_TreatmentRecord_Regulation_TreatmentRecord_TreatmentRecordId] FOREIGN KEY([TreatmentRecordId])
REFERENCES [dbo].[TreatmentRecord] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TreatmentRecord_Regulation] CHECK CONSTRAINT [FK_TreatmentRecord_Regulation_TreatmentRecord_TreatmentRecordId]
GO
ALTER TABLE [dbo].[TreatmentRecordDetail]  WITH CHECK ADD  CONSTRAINT [FK_TreatmentRecordDetail_Room_RoomId] FOREIGN KEY([RoomId])
REFERENCES [dbo].[Room] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TreatmentRecordDetail] CHECK CONSTRAINT [FK_TreatmentRecordDetail_Room_RoomId]
GO
ALTER TABLE [dbo].[TreatmentRecordDetail]  WITH CHECK ADD  CONSTRAINT [FK_TreatmentRecordDetail_TreatmentRecord_TreatmentRecordId] FOREIGN KEY([TreatmentRecordId])
REFERENCES [dbo].[TreatmentRecord] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TreatmentRecordDetail] CHECK CONSTRAINT [FK_TreatmentRecordDetail_TreatmentRecord_TreatmentRecordId]
GO
ALTER TABLE [dbo].[TreatmentTracking]  WITH CHECK ADD  CONSTRAINT [FK_TreatmentTracking_TreatmentRecordDetail_TreatmentRecordDetailId] FOREIGN KEY([TreatmentRecordDetailId])
REFERENCES [dbo].[TreatmentRecordDetail] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TreatmentTracking] CHECK CONSTRAINT [FK_TreatmentTracking_TreatmentRecordDetail_TreatmentRecordDetailId]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Employee_EmployeeId] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Employee_EmployeeId]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Patient_PatientId] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Patient_PatientId]
GO
USE [master]
GO
ALTER DATABASE [TraditionalMedicineHospitalDB] SET  READ_WRITE 
GO
