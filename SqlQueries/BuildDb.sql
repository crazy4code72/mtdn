-- Create database named Matdaan and then create the following tables.
    USE Matdaan
    GO

    CREATE TABLE VOTING(
        VOTER_ID NVARCHAR (10) NOT NULL,
        NAME NVARCHAR (50) NOT NULL,
        FATHER_NAME NVARCHAR (50) NOT NULL,
        DOB NVARCHAR(10) NOT NULL,
        GENDER NVARCHAR(10) NOT NULL,
        LINKED_TO_AADHAR BIT DEFAULT(0),
        CONSTRAINT [PK_VOTERID] PRIMARY KEY CLUSTERED (VOTER_ID ASC),
    )
    GO

    CREATE TABLE AADHAR(
        AADHAR_NO NVARCHAR(12) NOT NULL,
        NAME NVARCHAR(50) NOT NULL,
        FATHER_NAME NVARCHAR(50) NOT NULL,
        DOB NVARCHAR(50) NOT NULL,
        GENDER NVARCHAR(10) NOT NULL,
        CONTACT_NO NVARCHAR(10),
        EMAIL_ID NVARCHAR(50),
        OTP INT,
        VOTER_ID NVARCHAR(10) UNIQUE,
        CONSTRAINT [PK_AADHAR_NO] PRIMARY KEY CLUSTERED (AADHAR_NO ASC)
    )
    GO

    CREATE TABLE VOTING_RESULT(
        VOTER_ID NVARCHAR(10) PRIMARY KEY,
        VOTED_FOR NVARCHAR(50),
        FOREIGN KEY (VOTER_ID) REFERENCES VOTING(VOTER_ID)
    )
    GO

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
           ,NULL)
GO

*/