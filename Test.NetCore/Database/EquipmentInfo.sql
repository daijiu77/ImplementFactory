USE [DbTest]
GO

/****** Object:  Table [dbo].[EquipmentInfo]    Script Date: 2021/6/13 2:35:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[EquipmentInfo](
	[id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_EquipmentInfo_id]  DEFAULT (newid()),
	[height] [int] NULL,
	[width] [int] NULL,
	[equipmentName] [varchar](50) NULL,
	[code] [varchar](50) NULL,
	[cdatetime] [datetime] NULL CONSTRAINT [DF_EquipmentInfo_cdatetime]  DEFAULT (getdate()),
 CONSTRAINT [PK_EquipmentInfo] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


