IF (NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE (name = 'Capital')))
	CREATE DATABASE [Capital]

GO

USE [Capital];


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[LogEvent]', 'U') IS NOT NULL
DROP TABLE [dbo].[LogEvent];

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'LogEvent'
CREATE TABLE [dbo].[LogEvent](
	[Guid] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_LogEvent_Guid]  DEFAULT (newid()),
	[EnteredDate] [datetime] NULL CONSTRAINT [DF_LogEvent_EnteredDate]  DEFAULT (getdate()),
	[SessionNumber] [nvarchar](50) NULL,
	[Logger] [nvarchar](100) NULL,
	[LogDateTime] [nvarchar](100) NULL,
	[LogLevel] [nvarchar](100) NULL,
	[MachineName] [nvarchar](100) NULL,
	[UserName] [nvarchar](100) NULL,
	[LogMessage] [nvarchar](max) NULL,
	[FileId] [nvarchar](100) NULL,
	[FolderId] [nvarchar](100) NULL,
	[FilePath] [nvarchar](255) NULL,
	[RevisionNumber] [nvarchar](10) NULL,
	[LogException] [nvarchar](max) NULL,
	[XmlData] [xml] NULL
 CONSTRAINT [PK_LogEvent] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

IF OBJECT_ID(N'[dbo].[ExportedDocuments]', 'U') IS NOT NULL
DROP TABLE [dbo].[ExportedDocuments];

CREATE TABLE [dbo].[ExportedDocument] (
   [ItemId] [nvarchar](100) NOT NULL,	
   [ModifiedDate] [datetime] NOT NULL,
   [DocumentType] [int] NOT NULL,
	CONSTRAINT [PK_dbo.ExportedDocuments] PRIMARY KEY CLUSTERED ([ItemId], [DocumentType]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
) ON [PRIMARY];

GO

IF OBJECT_ID(N'[dbo].[MessageLog]', 'U') IS NOT NULL
DROP TABLE [dbo].[MessageLog];

CREATE TABLE [dbo].[MessageLog] (
   [ID] [int] IDENTITY(1,1) NOT NULL,
   [MachineName] [nvarchar](200) NULL,
   [UserName] [nvarchar](100) NULL,
   [EnteredDate] [datetime] NULL CONSTRAINT [DF_MessageLog_EnteredDate]  DEFAULT (getdate()),
   [LogDateTime] [nvarchar](100) NULL,
   [Logger] [nvarchar](100) NULL,
   [SessionNumber] [nvarchar](50) NULL,
   [MessageType] [nvarchar](50) NULL,
   [XmlData] [xml] NOT NULL,
 CONSTRAINT [PK_dbo.MessageLog] PRIMARY KEY CLUSTERED ([ID] ASC) 
   WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

GO


