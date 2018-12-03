var app = angular.module('VotingApp', ['ui.bootstrap']);
app.run(function () { });

app.controller('VotingAppController', ['$rootScope', '$scope', '$http', '$timeout', function ($rootScope, $scope, $http, $timeout) {
    // Refresh the webpage.
    $scope.refresh = function () {
        $http.get('api/Votes?c=' + new Date().getTime())
            .then(function (data, status) {
                $scope.votes = data;
            }, function (data, status) {
                $scope.votes = undefined;
            });
    };

    // Delete the vote.
    $scope.remove = function (item) {
        $http.delete('api/Votes/' + item)
            .then(function(data, status) {
                $scope.refresh();
            });
    };

    // Add the vote.
    $scope.add = function (item) {
        var fd = new FormData();
        fd.append('item', item);
        $http.put('api/Votes/' + item, fd, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .then(function (data, status) {
            $scope.refresh();
            $scope.item = undefined;
        });
    };

    // Submit Aadhar no to send otp to registered contact no and email id.
    $scope.SubmitAadharNoToSendOtp = function (aadharNo) {
        $scope.update("none", "none");
        if (aadharNo === undefined || aadharNo.toString().length !== 12) {
            alert("Invalid Aadhar No");
            return;
        }
        console.log("SubmitAadharNoToSendOtp requested");

        $http.post('api/Votes/SubmitAadharNoToSendOtp/' + aadharNo, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .then(function (response) {
            // Use status and data to notify user accordingly.
            $scope.item = undefined;
            if (response.data === "Success") {
                $scope.OtpSendingMessage = "OTP sent to registered mobile no and email id.";
                $scope.OtpSendingStatus = "green";
                $scope.update("block", "none");
                console.log("SubmitAadharNoToSendOtp success response received");
            } else {
                $scope.OtpSendingMessage = "OTP sending to registered mobile no and email id failed, please try again.";
                $scope.OtpSendingStatus = "red";
                $scope.update("none", "none");
                console.log("SubmitAadharNoToSendOtp failure response received");
            }
        });
    };

    // Verify otp for Aadhar No.
    $scope.VerifyOtp = function (aadharNo, userEnteredOtp) {
        if (aadharNo === undefined || aadharNo.toString().length !== 12 || userEnteredOtp === undefined || userEnteredOtp.toString().length !== 6) {
            alert("Invalid OTP");
            return;
        }
        console.log("VerifyOtp requested");

        $http.post('api/Votes/VerifyOtp/' + aadharNo + '/' + userEnteredOtp, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .then(function (response) {
            if (response.data === "Success") {
                $scope.OtpVerificationMessage = "OTP verified successfully.";
                $scope.OtpVerificationStatus = "green";
                $scope.update("block", "block");
                document.getElementById("txtAddAadharNo").disabled = true;
                console.log("VerifyOtp success response received");
            } else {
                $scope.OtpVerificationMessage = "Incorrect OTP.";
                $scope.OtpVerificationStatus = "red";
                $scope.update("block", "none");
                console.log("VerifyOtp failure response received");
            }
        });
    };

    // Link Voter Id to Aadhar.
    $scope.LinkVoterIdToAadhar = function (aadharNo, voterId, name, dob, fatherName, gender) {
        if (aadharNo === undefined || aadharNo.toString().length !== 12 || voterId === undefined || voterId.length !== 10 ||
            name === undefined || dob === undefined || dob.length !== 10 || fatherName === undefined || gender === undefined) {
            alert("Invalid Voter Id details");
            return;
        }
        console.log("Voter Id successfully linked to Aadhar");
    };

    // Update flags/status for the UI elements etc...
    $scope.update = function (enterOtpDivDisplay, enterVoterIdDivDisplay) {
        if (enterOtpDivDisplay !== undefined) {
            document.getElementById("EnterOtpDiv").style.display = enterOtpDivDisplay;
        }
        if (enterVoterIdDivDisplay !== undefined) {
            document.getElementById("EnterVoterIdDiv").style.display = enterVoterIdDivDisplay;
        }
    };
}]);