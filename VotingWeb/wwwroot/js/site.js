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
        $scope.update("none", "none", false);
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
                $scope.update("block", "none", false);
                console.log("SubmitAadharNoToSendOtp success response received");
            } else {
                $scope.OtpSendingMessage = "OTP sending to registered mobile no and email id failed, please try again.";
                $scope.OtpSendingStatus = "red";
                $scope.update("none", "none", false);
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
                $scope.update("block", "block", true);
                console.log("VerifyOtp success response received");
            } else {
                $scope.OtpVerificationMessage = "Incorrect OTP.";
                $scope.OtpVerificationStatus = "red";
                $scope.update("block", "none", false);
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

        console.log("Voter Id linking to Aadhar initiated");
        var json = {
            "AadharNo": aadharNo,
            "VoterId": voterId,
            "Name": name,
            "DOB": dob,
            "FatherName": fatherName,
            "Gender": gender
        };
        var payloadData = JSON.stringify(json);
        $http({
            method: 'POST',
            url: 'api/Votes/LinkVoterIdToAadhar',
            data: payloadData,
            headers: { 'Content-Type': 'application/json; charset=utf-8' }
        })
        .then(function (response) {
            $scope.update("block", "block", true);
            if (response.data === "SuccessfullyLinked") {
                $scope.updateVoterIdLinkMsgAndColor("Voter Id successfully linked to Aadhar.", "green");
            }
            else if (response.data === "AlreadyLinked") {
                $scope.updateVoterIdLinkMsgAndColor("Voter Id is already linked to Aadhar.", "blue");
            }
            else if (response.data === "LinkingFailed") {
                $scope.updateVoterIdLinkMsgAndColor("Incorrect Voter Id, failed to link Voter Id to Aadhar.", "red");
            }
            else if (response.data === "Unauthorized") {
                $scope.updateVoterIdLinkMsgAndColor("Unauthorized user, failed to link Voter Id to Aadhar.", "red");
            }
        });
    };

    // Update flags/status for the UI elements etc...
    $scope.update = function (enterOtpDivDisplay, enterVoterIdDivDisplay, disableAadharTextField) {
        if (enterOtpDivDisplay !== undefined) {
            document.getElementById("EnterOtpDiv").style.display = enterOtpDivDisplay;
        }
        if (enterVoterIdDivDisplay !== undefined) {
            document.getElementById("EnterVoterIdDiv").style.display = enterVoterIdDivDisplay;
        }
        if (disableAadharTextField !== undefined) {
            document.getElementById("txtAddAadharNo").disabled = disableAadharTextField;
        }
    };

    // Update voter id link status and message text color.
    $scope.updateVoterIdLinkMsgAndColor = function (msg, color) {
        $scope.VoterIdLinkingMessage = msg;
        $scope.VoterIdLinkingStatus = color;
        console.log(msg);
    };
}]);