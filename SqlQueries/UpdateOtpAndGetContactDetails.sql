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