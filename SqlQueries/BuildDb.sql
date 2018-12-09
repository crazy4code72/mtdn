    CREATE DATABASE Matdaan
    GO

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
        VOTER_ID NVARCHAR(10),
        CONSTRAINT [PK_AADHAR_NO] PRIMARY KEY CLUSTERED (AADHAR_NO ASC)
    )
    GO

    CREATE TABLE VOTING_RESULT(
        VOTER_ID NVARCHAR(10) PRIMARY KEY,
        VOTE_FOR NVARCHAR(50),
    )
    GO

    CREATE PROCEDURE CastVote
        @AADHAR_NO NVARCHAR(12),
        @VOTER_ID NVARCHAR(10),
        @VOTE_FOR NVARCHAR(50),
        @OTP INT
    AS
    BEGIN
        DECLARE @USER_AUTHORIZED_TO_VOTE BIT = 0;
        DECLARE @OVERRIDE_VOTE BIT = 0;

        SELECT @USER_AUTHORIZED_TO_VOTE = 1 FROM AADHAR
        WHERE AADHAR_NO = @AADHAR_NO AND VOTER_ID = @VOTER_ID AND OTP = @OTP;

        IF @USER_AUTHORIZED_TO_VOTE = 1
        BEGIN
            SELECT @OVERRIDE_VOTE = 1 FROM VOTING_RESULT WHERE VOTER_ID = @VOTER_ID;
            IF @OVERRIDE_VOTE = 1
            BEGIN
               UPDATE VOTING_RESULT SET VOTE_FOR = @VOTE_FOR WHERE VOTER_ID = @VOTER_ID;
            END
            ELSE
            BEGIN
               INSERT INTO VOTING_RESULT(VOTER_ID, VOTE_FOR) VALUES(@VOTER_ID, @VOTE_FOR);
            END
        END
    END
GO

    CREATE PROCEDURE UpdateOtpAndGetContactDetails
        @AADHAR_NO NVARCHAR(12),
        @OTP INT
    AS
    BEGIN
        UPDATE AADHAR
        SET OTP = @OTP
        WHERE AADHAR_NO = @AADHAR_NO;
    
        SELECT CONTACT_NO, EMAIL_ID FROM AADHAR
        WHERE AADHAR_NO = @AADHAR_NO;
    END
GO

    CREATE PROCEDURE VerifyOtp
        @AADHAR_NO NVARCHAR(12),
        @OTP INT
    AS
    BEGIN
        DECLARE @OTP_VERIFIED BIT = 0;
        SELECT @OTP_VERIFIED = 1 FROM AADHAR WHERE AADHAR_NO = @AADHAR_NO AND OTP = @OTP;
        SELECT @OTP_VERIFIED AS OTP_VERIFIED;
    END;
GO

    CREATE PROCEDURE LinkVoterIdToAadhar
        @AADHAR_NO NVARCHAR(12),
        @VOTER_ID NVARCHAR(10),
        @NAME NVARCHAR(50),
        @DOB NVARCHAR(10),
        @FATHER_NAME NVARCHAR(50),
        @GENDER NVARCHAR(10),
        @OTP INT
    AS
    BEGIN
    
    DECLARE @OTP_VERIFIED BIT = 0;
    DECLARE @LINKED_TO_AADHAR BIT = 0;
    DECLARE @VOTER_ID_MATCHED BIT = 0;
    DECLARE @LINKING_SUCCESSFUL BIT = 0;
    DECLARE @IDENTITY_MATCHED BIT = 0;
    
    SELECT * INTO #AADHAR_TEMP FROM AADHAR WHERE AADHAR_NO = @AADHAR_NO;
    SELECT * INTO #VOTING_TEMP FROM VOTING WHERE VOTER_ID = @VOTER_ID;
    
    SELECT @OTP_VERIFIED = CASE WHEN @OTP = OTP THEN 1 ELSE 0 END FROM #AADHAR_TEMP;
    SELECT @LINKED_TO_AADHAR = LINKED_TO_AADHAR FROM #VOTING_TEMP;
    
    IF @OTP_VERIFIED = 1
    BEGIN
        IF @LINKED_TO_AADHAR = 0
        BEGIN
            BEGIN TRANSACTION
                SELECT @IDENTITY_MATCHED = 1 FROM #AADHAR_TEMP a
                CROSS JOIN #VOTING_TEMP v
                WHERE a.NAME = v.NAME OR a.FATHER_NAME = v.FATHER_NAME OR a.DOB = v.dob;
    
                IF @IDENTITY_MATCHED = 1
                BEGIN
                    UPDATE VOTING
                    SET LINKED_TO_AADHAR = 1, @VOTER_ID_MATCHED = 1
                    WHERE VOTER_ID = @VOTER_ID AND NAME = @NAME AND FATHER_NAME = @FATHER_NAME AND GENDER = @GENDER AND DOB = @DOB;
    
                    IF @VOTER_ID_MATCHED = 1
                    BEGIN
                        UPDATE AADHAR
                        SET VOTER_ID = @VOTER_ID, @LINKING_SUCCESSFUL = 1
                        WHERE AADHAR_NO = @AADHAR_NO;
                    END
    
                    IF @LINKING_SUCCESSFUL = 1
                    BEGIN
                        SELECT 'SuccessfullyLinked' AS VOTER_ID_LINKING_STATUS;
                    END
                    ELSE
                    BEGIN
                        SELECT 'LinkingFailed' AS VOTER_ID_LINKING_STATUS;
                    END
                END
                ELSE
                BEGIN
                    SELECT 'LinkingFailed' AS VOTER_ID_LINKING_STATUS;
                END
            COMMIT
        END
        ELSE
        BEGIN
            SELECT 'AlreadyLinked' AS VOTER_ID_LINKING_STATUS;
        END
    END
    ELSE
    BEGIN
        SELECT 'Unauthorized' AS VOTER_ID_LINKING_STATUS;
    END
    
    DROP TABLE #AADHAR_TEMP;
    DROP TABLE #VOTING_TEMP;
    
    END
GO

/*
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
           ,'Mayank'
           ,'Devendra Kumar Sinha'
           ,'01-12-1991'
           ,'Male'
           ,'8951471517'
           ,'zmsrms@gmail.com'
           ,NULL
           ,NULL),
           ('123456789001'
           ,'Shekhar'
           ,'D K Sinha'
           ,'01-12-1991'
           ,'Male'
           ,'8951471517'
           ,'zmsrms@gmail.com'
           ,NULL
           ,NULL)
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
           ,'Mayank'
           ,'Devendra Kumar Sinha'
           ,'01/12/1991'
           ,'Male'
           ,0),
           ('1234567891'
           ,'Shekhar'
           ,'D K Sinha'
           ,'01/12/1991'
           ,'Male'
           ,0)
GO*/
