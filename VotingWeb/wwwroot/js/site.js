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
        if (aadharNo === undefined || aadharNo.toString().length !== 12) {
            $scope.updateAadharElements("block", false, "Invalid Aadhar No.", "red");
            $scope.updateOtpElements("none", undefined, undefined, undefined);
            $scope.updateVoterCardElements("none", undefined, undefined, undefined);
            return;
        }

        $http.post('api/Votes/SubmitAadharNoToSendOtp/' + aadharNo, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .then(function (response) {
            if (response.data === "Success") {
                $scope.updateAadharElements("block", false, "OTP sent to registered mobile no and email id.", "green");
                $scope.updateOtpElements("block", false, undefined, undefined);
                $scope.updateVoterCardElements("none", undefined, undefined, undefined);
            } else {
                $scope.updateAadharElements("block", false, "OTP sending to registered mobile no and email id failed, please try again.", "red");
                $scope.updateOtpElements("none", undefined, undefined, undefined);
                $scope.updateVoterCardElements("none", undefined, undefined, undefined);
            }
        });
    };

    // Verify otp for Aadhar No.
    $scope.VerifyOtp = function (aadharNo, userEnteredOtp) {
        if (aadharNo === undefined || aadharNo.toString().length !== 12 || userEnteredOtp === undefined || userEnteredOtp.toString().length !== 6) {
            $scope.updateAadharElements("block", false, undefined, undefined);
            $scope.updateOtpElements("block", false, "Incorrect OTP.", "red");
            $scope.updateVoterCardElements("none", undefined, undefined, undefined);
            return;
        }

        $http.post('api/Votes/VerifyOtp/' + aadharNo + '/' + userEnteredOtp, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .then(function (response) {
            if (response.data === "Success") {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, "OTP verified successfully.", "green");
                $scope.updateVoterCardElements("block", false, undefined, undefined);
            } else {
                $scope.updateAadharElements("block", false, undefined, undefined);
                $scope.updateOtpElements("block", false, "Incorrect OTP.", "red");
                $scope.updateVoterCardElements("none", undefined, undefined, undefined);
            }
        });
    };

    // Link Voter Id to Aadhar.
    $scope.LinkVoterIdToAadhar = function (aadharNo, voterId, name, dob, fatherName, gender, userEnteredOtp) {
        if (aadharNo === undefined || aadharNo.toString().length !== 12 || voterId === undefined || voterId.length !== 10 ||
            name === undefined || dob === undefined || dob.length !== 10 || fatherName === undefined || gender === undefined ||
            userEnteredOtp === undefined || userEnteredOtp.toString().length !== 6) {
            $scope.updateAadharElements("block", true, undefined, undefined);
            $scope.updateOtpElements("block", true, undefined, undefined);
            $scope.updateVoterCardElements("block", false, "Invalid Voter card details.", "red");
            return;
        }

        var payload = { "AadharNo": aadharNo, "VoterId": voterId, "Name": name, "DOB": dob, "FatherName": fatherName, "Gender": gender, "Otp": userEnteredOtp };

        $http({
            method: 'POST',
            url: 'api/Votes/LinkVoterIdToAadhar',
            data: JSON.stringify(payload),
            headers: { 'Content-Type': 'application/json; charset=utf-8' }
        })
        .then(function (response) {
            if (response.data === "SuccessfullyLinked") {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", true, "Voter Id successfully linked to Aadhar.", "green");
            }
            else if (response.data === "AlreadyLinked") {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", true, "Voter Id is already linked to Aadhar.", "blue");
            }
            else if (response.data === "LinkingFailed") {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", false, "Incorrect Voter Id, failed to link Voter Id to Aadhar.", "red");
            }
            else if (response.data === "Unauthorized") {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", false, "Unauthorized user, failed to link Voter Id to Aadhar.", "red");
            }
            else
            {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", false, "Something went wrong, please try again.", "red");
            }
        });
    };

    // Update Aadhar UI elements.
    $scope.updateAadharElements = function (aadharDivDisplay, disableAadharElements, aadharSubmissionMsg, aadharSubmissionColor) {
        if (aadharDivDisplay !== undefined) {
            document.getElementById("EnterAadharNoDiv").style.display = aadharDivDisplay;
        }
        if (disableAadharElements !== undefined) {
            document.getElementById("txtAddAadharNo").disabled = disableAadharElements;
            document.getElementById("btnAddAadharNo").disabled = disableAadharElements;
        }
        if (aadharSubmissionMsg !== undefined) {
            $scope.AadharSubmissionMessage = aadharSubmissionMsg;
            console.log(aadharSubmissionMsg);
        }
        if (aadharSubmissionColor !== undefined) {
            $scope.AadharSubmissionColor = aadharSubmissionColor;
        }
    };

    // Update OTP UI elements.
    $scope.updateOtpElements = function (otpDivDisplay, disableOtpElements, otpStatusMsg, otpStatusColor) {
        if (otpDivDisplay !== undefined) {
            document.getElementById("EnterOtpDiv").style.display = otpDivDisplay;
        }
        if (disableOtpElements !== undefined) {
            document.getElementById("txtEnterOtp").disabled = disableOtpElements;
            document.getElementById("btnVerifyOtp").disabled = disableOtpElements;
        }
        if (otpStatusMsg !== undefined) {
            $scope.OtpVerificationMessage = otpStatusMsg;
            console.log(otpStatusMsg);
        }
        if (otpStatusColor !== undefined) {
            $scope.OtpVerificationColor = otpStatusColor;
        }
    };

    // Update Voter card UI elements.
    $scope.updateVoterCardElements = function (voterCardDivDisplay, disableVoterCardElements, voterIdLinkMsg, voterIdLinkColor) {
        if (voterCardDivDisplay !== undefined) {
            document.getElementById("EnterVoterIdDiv").style.display = voterCardDivDisplay;
        }
        if (disableVoterCardElements !== undefined) {
            document.getElementById("txtEnterVoterId").disabled = disableVoterCardElements;
            document.getElementById("txtEnterName").disabled = disableVoterCardElements;
            document.getElementById("txtEnterFatherName").disabled = disableVoterCardElements;
            document.getElementById("txtEnterDOB").disabled = disableVoterCardElements;
            document.getElementById("txtEnterGender").disabled = disableVoterCardElements;
            document.getElementById("btnLinkVoterIdToAadhar").disabled = disableVoterCardElements;
        }
        if (voterIdLinkMsg !== undefined) {
            $scope.VoterIdLinkingMessage = voterIdLinkMsg;
            console.log(voterIdLinkMsg);
        }
        if (voterIdLinkColor !== undefined) {
            $scope.VoterIdLinkingColor = voterIdLinkColor;
        }
    };
}]);