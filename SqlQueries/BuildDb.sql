-- Create database named Matdaan and then create the following tables.
    CREATE TABLE VOTING(
        VOTER_ID NVARCHAR (10) NOT NULL,
        NAME NVARCHAR (50) NOT NULL,
        FATHER_NAME NVARCHAR (50) NOT NULL,
        DOB NVARCHAR(10) NOT NULL,
        GENDER NVARCHAR(10) NOT NULL,
        LINKED_TO_AADHAR BIT DEFAULT(0),
        CONSTRAINT [PK_VOTERID] PRIMARY KEY CLUSTERED (VOTER_ID ASC),
    );

    CREATE TABLE AADHAR(
        AADHAR_NO NVARCHAR(12) NOT NULL,
        NAME NVARCHAR(50) NOT NULL,
        FATHER_NAME NVARCHAR(50) NOT NULL,
        DOB NVARCHAR(50) NOT NULL,
        GENDER NVARCHAR(10) NOT NULL,
        CONTACT_NO NVARCHAR(10),
        EMAIL_ID NVARCHAR(50),
        OTP INT,
        OTP_VERIFIED BIT DEFAULT(0),
        VOTER_ID NVARCHAR(20) UNIQUE,
        CONSTRAINT [PK_AADHAR_NO] PRIMARY KEY CLUSTERED (AADHAR_NO ASC),
    );

/* Insert dummy data
USE [Matdaan]
GO

INSERT INTO [dbo].[VOTING]
           ([VOTER_ID]
           ,[NAME]
           ,[FATHER_NAME]
           ,[DOB]
           ,[GENDER]
           ,[LINKED_TO_AADHAR])
     VALUES
           ('1234567890'
           ,'Mayank Shekhar'
           ,'Devendra Kumar Sinha'
           ,'01/12/1991'
           ,'Male'
           ,0)
GO

INSERT INTO [dbo].[AADHAR]
           ([AADHAR_NO]
           ,[NAME]
           ,[FATHER_NAME]
           ,[DOB]
           ,[GENDER]
           ,[CONTACT_NO]
           ,[EMAIL_ID]
           ,[OTP]
           ,[OTP_VERIFIED]
           ,[VOTER_ID])
     VALUES
           ('123456789000'
           ,'Mayank Shekhar'
           ,'Devendra Kumar Sinha'
           ,'01-12-1991'
           ,'Male'
           ,'8951471517'
           ,'zmsrms@gmail.com'
           ,NULL
           ,0
           ,NULL)
GO

*/