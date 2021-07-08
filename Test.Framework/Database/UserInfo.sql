USE [DbTest]
GO

/****** Object:  Table [dbo].[UserInfo]    Script Date: 2021/6/13 2:35:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[UserInfo](
	[id] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[name] [varchar](50) NULL,
	[age] [int] NULL,
	[address] [varchar](500) NULL,
	[cdatetime] [datetime] NULL DEFAULT (CONVERT([varchar](50),getdate(),(121))),
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


