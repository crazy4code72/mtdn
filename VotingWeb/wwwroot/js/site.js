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
        $scope.updateAadharAndOtpElements("none", "none", false, undefined, undefined, undefined, undefined);
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
                $scope.updateAadharAndOtpElements("block", "none", false, undefined, undefined, undefined, undefined);
                console.log("SubmitAadharNoToSendOtp success response received");
            } else {
                $scope.OtpSendingMessage = "OTP sending to registered mobile no and email id failed, please try again.";
                $scope.OtpSendingStatus = "red";
                $scope.updateAadharAndOtpElements("none", "none", false, undefined, undefined, undefined, undefined);
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
                $scope.updateAadharAndOtpElements("block", "block", true, undefined, true, true, true);
                console.log("VerifyOtp success response received");
            } else {
                $scope.OtpVerificationMessage = "Incorrect OTP.";
                $scope.OtpVerificationStatus = "red";
                $scope.updateAadharAndOtpElements("block", "none", false, undefined, false, false, false);
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
            if (response.data === "SuccessfullyLinked") {
                $scope.updateVoterIdLinkMsgAndColor("Voter Id successfully linked to Aadhar.", "green");
                $scope.updateVoterCardElements(true, "block");
            }
            else if (response.data === "AlreadyLinked") {
                $scope.updateVoterIdLinkMsgAndColor("Voter Id is already linked to Aadhar.", "blue");
                $scope.updateVoterCardElements(true, "block");
            }
            else if (response.data === "LinkingFailed") {
                $scope.updateVoterIdLinkMsgAndColor("Incorrect Voter Id, failed to link Voter Id to Aadhar.", "red");
                $scope.updateVoterCardElements(false, "block");
            }
            else if (response.data === "Unauthorized") {
                $scope.updateVoterIdLinkMsgAndColor("Unauthorized user, failed to link Voter Id to Aadhar.", "red");
                $scope.updateVoterCardElements(false, "block");
            }
            else
            {
                $scope.updateVoterIdLinkMsgAndColor("Something went wrong, please try again.", "red");
                $scope.updateVoterCardElements(false, "block");
            }
        });
    };

    // Update flags/status for the Aadhar and Otp elements.
    $scope.updateAadharAndOtpElements = function (enterOtpDivDisplay, enterVoterIdDivDisplay, disableAadharTextField, disableVoterIdTextField,
                                                  disableVerifyOtpButton, disableSubmitAadharButton, disableOtpTextField) {
        if (enterOtpDivDisplay !== undefined) {
            document.getElementById("EnterOtpDiv").style.display = enterOtpDivDisplay;
        }
        if (enterVoterIdDivDisplay !== undefined) {
            document.getElementById("EnterVoterIdDiv").style.display = enterVoterIdDivDisplay;
        }
        if (disableAadharTextField !== undefined) {
            document.getElementById("txtAddAadharNo").disabled = disableAadharTextField;
        }
        if (disableVoterIdTextField !== undefined) {
            document.getElementById("txtEnterVoterId").disabled = disableVoterIdTextField;
        }
        if (disableVerifyOtpButton !== undefined) {
            document.getElementById("btnVerifyOtp").disabled = disableVerifyOtpButton;
        }
        if (disableSubmitAadharButton !== undefined) {
            document.getElementById("btnAddAadharNo").disabled = disableSubmitAadharButton;
        }
        if (disableOtpTextField !== undefined) {
            document.getElementById("txtEnterOtp").disabled = disableOtpTextField;
        }
    };

    // Update flags/status for the Voter card elements.
    $scope.updateVoterCardElements = function (disableVoterCardElements, hideVoterCardDiv) {
        if (hideVoterCardDiv !== undefined) {
            document.getElementById("EnterVoterIdDiv").style.display = hideVoterCardDiv;
        }
        if (disableVoterCardElements !== undefined) {
            document.getElementById("txtEnterVoterId").disabled = disableVoterCardElements;
            document.getElementById("txtEnterName").disabled = disableVoterCardElements;
            document.getElementById("txtEnterFatherName").disabled = disableVoterCardElements;
            document.getElementById("txtEnterDOB").disabled = disableVoterCardElements;
            document.getElementById("txtEnterGender").disabled = disableVoterCardElements;
            document.getElementById("btnLinkVoterIdToAadhar").disabled = disableVoterCardElements;
        }
    };

    // Update voter id link status and message text color.
    $scope.updateVoterIdLinkMsgAndColor = function (msg, color) {
        $scope.VoterIdLinkingMessage = msg;
        $scope.VoterIdLinkingStatus = color;
        console.log(msg);
    };
}]);