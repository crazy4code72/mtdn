﻿var app = angular.module('VotingApp', ['ui.bootstrap']);
app.run(function () { });

app.controller('VotingAppController', ['$rootScope', '$scope', '$http', '$timeout', function ($rootScope, $scope, $http, $timeout) {

    $scope.candidates = [];
    $scope.FetchLiveVotingMessage = "Fetching current voting result, please wait...";

    $scope.LiveVotingResult = function () {
        $scope.updateAadharElements("none", undefined, undefined, undefined);
        $scope.updateOtpElements("none", undefined, undefined, undefined);
        $scope.updateVoterCardElements("none", undefined, undefined, undefined);
        $scope.updateCastVoteElements("none", undefined, undefined, undefined);

        $scope.FetchLiveVotingResult();
        var interval = setInterval(function () {
            $scope.FetchLiveVotingResult();
        }, 10000);
    };

    $scope.FetchLiveVotingResult = function () {
        $scope.FetchLiveVotingMessage = "Fetching current voting result, please wait...";
        $http.get('api/Votes/LiveVotingResult')
            .then(function(response) {
                if (response === null || response === undefined || response.data.length === 0) {
                    $scope.FetchLiveVotingMessage = "No vote casted for any candidate.";
                    $scope.votes = { "data": [] };
                } else if (response.data === "TooManyTries") {
                    // Don't change the current voting data, try to get latest voting data in next call after 10 seconds.
                    $scope.FetchLiveVotingMessage = "Too many requests from your device, please keep only one tab open.";
                    $scope.votes = { "data": [] };
                } else {
                    $scope.FetchLiveVotingMessage = "Current voting status.";
                    $scope.votes = response;
                }
                document.getElementById("LiveVotingResult").style.display = "block";
            });
    };

    // Delete the vote.
//    $scope.remove = function (item) {
//        $http.delete('api/Votes/' + item)
//            .then(function(data, status) {
//                $scope.refresh();
//            });
//    };

    // Add the vote.
//    $scope.add = function (item) {
//        var fd = new FormData();
//        fd.append('item', item);
//        $http.put('api/Votes/' + item, fd, {
//            transformRequest: angular.identity,
//            headers: { 'Content-Type': undefined }
//        })
//        .then(function (data, status) {
//            $scope.refresh();
//            $scope.item = undefined;
//        });
//    };

    // Start fresh again.
    $scope.StartFresh = function() {
        window.location.reload();
    };

    $scope.refresh = function () {
        $scope.GenerateCaptcha();
    };

    $scope.GenerateCaptcha = function () {
        $scope.CaptchaText = Math.floor(Math.random() * 10) + ' ' + Math.floor(Math.random() * 10) + ' ' + Math.floor(Math.random() * 10) + ' ' +
                             Math.floor(Math.random() * 10) + ' ' + Math.floor(Math.random() * 10) + ' ' + Math.floor(Math.random() * 10) + ' ' +
                             Math.floor(Math.random() * 10);
    };

    // Submit Aadhar no to send otp to registered contact no and email id.
    $scope.SubmitAadharNoToSendOtp = function (aadharNo, userEnteredCaptchaText) {
        if (aadharNo === undefined || aadharNo.toString().trim().length !== 12 || parseInt(aadharNo) > 999999999999)
        {
            $scope.updateAadharElements("block", false, "Invalid Aadhar No.", "red");
            $scope.updateOtpElements("none", undefined, undefined, undefined);
            $scope.updateVoterCardElements("none", undefined, undefined, undefined);
            $scope.updateCastVoteElements("none", true, undefined, undefined);
            return;
        }

        if (userEnteredCaptchaText === undefined || userEnteredCaptchaText === "" ||
            userEnteredCaptchaText.split(' ').join('') !== $scope.CaptchaText.split(' ').join(''))
        {
            $scope.updateAadharElements("block", false, "Incorrect captcha text, click on the image to change and retry.", "red");
            $scope.updateOtpElements("none", undefined, undefined, undefined);
            $scope.updateVoterCardElements("none", undefined, undefined, undefined);
            $scope.updateCastVoteElements("none", true, undefined, undefined);
            return;
        }

        $scope.updateAadharElements("block", true, "Sending OTP to registered mobile no and email id...", "blue");

        $http.post('api/Votes/SubmitAadharNoToSendOtp/' + aadharNo, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .then(function (response) {
            if (response.data === "Success") {
                $scope.updateAadharElements("block", false, "OTP sent to registered mobile no and email id.", "#07bb07");
                $scope.updateOtpElements("block", false, undefined, undefined);
                $scope.updateVoterCardElements("none", undefined, undefined, undefined);
                $scope.updateCastVoteElements("none", true, undefined, undefined);
            } else if (response.data === "TooManyTries") {
                $scope.updateAadharElements("block", false, "Too many attempts, please try after 15 minutes.", "red");
                $scope.updateOtpElements("block", false, undefined, undefined);
                $scope.updateVoterCardElements("none", undefined, undefined, undefined);
                $scope.updateCastVoteElements("none", true, undefined, undefined);
            } else {
                $scope.updateAadharElements("block", false, "OTP sending failed, please try again.", "red");
                $scope.updateOtpElements("none", undefined, undefined, undefined);
                $scope.updateVoterCardElements("none", undefined, undefined, undefined);
                $scope.updateCastVoteElements("none", true, undefined, undefined);
            }
        });
    };

    // Verify otp for Aadhar No.
    $scope.VerifyOtp = function (aadharNo, userEnteredOtp) {
        if (aadharNo === undefined || aadharNo.toString().trim().length !== 12 || parseInt(aadharNo) > 999999999999 ||
            userEnteredOtp === undefined || userEnteredOtp.toString().trim().length !== 6 || parseInt(userEnteredOtp) > 999999 ||
            parseInt(userEnteredOtp) < 100000) {
            $scope.updateAadharElements("block", false, undefined, undefined);
            $scope.updateOtpElements("block", false, "Incorrect OTP.", "red");
            $scope.updateVoterCardElements("none", undefined, undefined, undefined);
            $scope.updateCastVoteElements("none", true, undefined, undefined);
            return;
        }

        $scope.updateOtpElements("block", true, "Verifying OTP, please wait...", "blue");

        $http.post('api/Votes/VerifyOtp/' + aadharNo + '/' + userEnteredOtp, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .then(function (response) {
            if (response.data === "Success") {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, "OTP verified successfully.", "#07bb07");
                $scope.updateVoterCardElements("block", false, undefined, undefined);
                $scope.updateCastVoteElements("none", true, undefined, undefined);
            } else if (response.data === "TooManyTries") {
                setTimeout(function () {
                   // Retry after 2 seconds.
                   $http.post('api/Votes/VerifyOtp/' + aadharNo + '/' + userEnteredOtp, {
                       transformRequest: angular.identity,
                       headers: { 'Content-Type': undefined }
                   })
                   .then(function (response) {
                       if (response.data === "Success") {
                           $scope.updateAadharElements("block", true, undefined, undefined);
                           $scope.updateOtpElements("block", true, "OTP verified successfully.", "#07bb07");
                           $scope.updateVoterCardElements("block", false, undefined, undefined);
                           $scope.updateCastVoteElements("none", true, undefined, undefined);
                       } else if (response.data === "Failure") {
                           $scope.updateAadharElements("block", false, undefined, undefined);
                           $scope.updateOtpElements("block", false, "Incorrect OTP.", "red");
                           $scope.updateVoterCardElements("none", undefined, undefined, undefined);
                           $scope.updateCastVoteElements("none", true, undefined, undefined);
                       } else {
                           $scope.updateAadharElements("block", false, undefined, undefined);
                           $scope.updateOtpElements("block", false, "Could not verify OTP, please retry after sometime.", "red");
                           $scope.updateVoterCardElements("none", undefined, undefined, undefined);
                           $scope.updateCastVoteElements("none", true, undefined, undefined);
                       }
                   });
                }, 2000);
            }
            else {
                $scope.updateAadharElements("block", false, undefined, undefined);
                $scope.updateOtpElements("block", false, "Incorrect OTP.", "red");
                $scope.updateVoterCardElements("none", undefined, undefined, undefined);
                $scope.updateCastVoteElements("none", true, undefined, undefined);
            }
        });
    };

    // Link Voter Id to Aadhar.
    $scope.LinkVoterIdToAadhar = function (aadharNo, voterId, name, dob, fatherName, gender, userEnteredOtp) {
        if (aadharNo === undefined || aadharNo.toString().trim().length !== 12 || parseInt(aadharNo) > 999999999999 ||
            voterId === undefined || voterId.toString().trim().length !== 10 || name === undefined || name.toString().trim() === "" ||
            dob === undefined || dob.toString().trim().length !== 10 || fatherName === undefined || fatherName.toString().trim() === "" ||
            gender === undefined || gender.toString().trim() === "" || userEnteredOtp === undefined ||
            userEnteredOtp.toString().trim().length !== 6 || parseInt(userEnteredOtp) > 999999 || parseInt(userEnteredOtp) < 100000) {
            $scope.updateAadharElements("block", true, undefined, undefined);
            $scope.updateOtpElements("block", true, undefined, undefined);
            $scope.updateVoterCardElements("block", false, "Invalid Voter card details.", "red");
            $scope.updateCastVoteElements("none", true, undefined, undefined);
            return;
        }

        $scope.updateVoterCardElements("block", true, "Trying to link Voter Id with Aadhar...", "blue");

        var payload = { "AadharNo": aadharNo, "VoterId": voterId, "Name": name, "DOB": dob, "FatherName": fatherName, "Gender": gender, "Otp": userEnteredOtp };

        $http({
            method: 'POST',
            url: 'api/Votes/LinkVoterIdToAadhar',
            data: JSON.stringify(payload),
            headers: { 'Content-Type': 'application/json; charset=utf-8' }
        })
        .then(function (response) {
            var splitResponse = response.data.split(':');
            if (splitResponse[0] === "SuccessfullyLinked") {
                if (splitResponse[1] !== "") {
                    $scope.candidates = splitResponse[1].split('#');
                    $scope.updateCastVoteElements("block", false, undefined, undefined);
                } else {
                    $scope.candidates = [];
                    $scope.updateCastVoteElements("none", true, undefined, undefined);
                }
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", true, "Voter Id successfully linked to Aadhar.", "#07bb07");
            }
            else if (splitResponse[0] === "AlreadyLinked") {
                if (splitResponse[1] !== "") {
                    $scope.candidates = splitResponse[1].split('#');
                    $scope.updateCastVoteElements("block", false, undefined, undefined);
                } else {
                    $scope.candidates = [];
                    $scope.updateCastVoteElements("none", true, undefined, undefined);
                }
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", true, "Voter Id is already linked to Aadhar.", "blue");
            }
            else if (response.data === "LinkingFailed") {
                $scope.candidates = [];
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", false, "Incorrect Voter details, failed to link Voter Id to Aadhar.", "red");
                $scope.updateCastVoteElements("none", true, undefined, undefined);
            }
            else if (response.data === "Unauthorized") {
                $scope.candidates = [];
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", false, "Unauthorized user, failed to link Voter Id to Aadhar.", "red");
                $scope.updateCastVoteElements("none", true, undefined, undefined);
            }
            else if (response.data === "TooManyTries") {
                 setTimeout(function () {
                     // Retry after 2 seconds.
                     $http({
                         method: 'POST',
                         url: 'api/Votes/LinkVoterIdToAadhar',
                         data: JSON.stringify(payload),
                         headers: { 'Content-Type': 'application/json; charset=utf-8' }
                     })
                     .then(function (response) {
                         var splitResponse = response.data.split(':');
                         if (splitResponse[0] === "SuccessfullyLinked") {
                             if (splitResponse[1] !== "") {
                                 $scope.candidates = splitResponse[1].split('#');
                                 $scope.updateCastVoteElements("block", false, undefined, undefined);
                             } else {
                                 $scope.candidates = [];
                                 $scope.updateCastVoteElements("none", true, undefined, undefined);
                             }
                             $scope.updateAadharElements("block", true, undefined, undefined);
                             $scope.updateOtpElements("block", true, undefined, undefined);
                             $scope.updateVoterCardElements("block", true, "Voter Id successfully linked to Aadhar.", "#07bb07");
                         }
                         else if (splitResponse[0] === "AlreadyLinked") {
                             if (splitResponse[1] !== "") {
                                 $scope.candidates = splitResponse[1].split('#');
                                 $scope.updateCastVoteElements("block", false, undefined, undefined);
                             } else {
                                 $scope.candidates = [];
                                 $scope.updateCastVoteElements("none", true, undefined, undefined);
                             }
                             $scope.updateAadharElements("block", true, undefined, undefined);
                             $scope.updateOtpElements("block", true, undefined, undefined);
                             $scope.updateVoterCardElements("block", true, "Voter Id is already linked to Aadhar.", "blue");
                         }
                         else if (response.data === "LinkingFailed") {
                             $scope.candidates = [];
                             $scope.updateAadharElements("block", true, undefined, undefined);
                             $scope.updateOtpElements("block", true, undefined, undefined);
                             $scope.updateVoterCardElements("block", false, "Incorrect Voter details, failed to link Voter Id to Aadhar.", "red");
                             $scope.updateCastVoteElements("none", true, undefined, undefined);
                         }
                         else if (response.data === "Unauthorized") {
                             $scope.candidates = [];
                             $scope.updateAadharElements("block", true, undefined, undefined);
                             $scope.updateOtpElements("block", true, undefined, undefined);
                             $scope.updateVoterCardElements("block", false, "Unauthorized user, failed to link Voter Id to Aadhar.", "red");
                             $scope.updateCastVoteElements("none", true, undefined, undefined);
                         }
                         else {
                             $scope.candidates = [];
                             $scope.updateAadharElements("block", true, undefined, undefined);
                             $scope.updateOtpElements("block", true, undefined, undefined);
                             $scope.updateVoterCardElements("block", false, "Could not link Voter Id, please try after sometime.", "red");
                             $scope.updateCastVoteElements("none", true, undefined, undefined);
                         }
                     });
                 }, 2000);
            }
            else
            {
                $scope.candidates = [];
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", false, "Something went wrong, please try again.", "red");
                $scope.updateCastVoteElements("none", true, undefined, undefined);
            }
        });
    };

    // Submit Vote along with voter id and other details.
    $scope.CastVote = function (aadharNo, voterId, castVoteFor, userEnteredOtp) {
        if (aadharNo === undefined || aadharNo.toString().trim().length !== 12 || parseInt(aadharNo) > 999999999999 ||
            voterId === undefined || voterId.toString().trim().length !== 10 || castVoteFor === undefined ||
            castVoteFor.toString().trim() === "" || userEnteredOtp === undefined || userEnteredOtp.toString().trim().length !== 6 ||
            parseInt(userEnteredOtp) > 999999 || parseInt(userEnteredOtp) < 100000) {
            $scope.updateAadharElements("block", true, undefined, undefined);
            $scope.updateOtpElements("block", true, undefined, undefined);
            $scope.updateVoterCardElements("block", true, undefined, undefined);
            $scope.updateCastVoteElements("block", false, "Voting failed, please try again.", "red");
            return;
        }

        if (!confirm("Are you sure you want to vote for " + castVoteFor + " ?")) return;

        $scope.updateCastVoteElements("block", true, "Submitting your vote, please wait...", "blue");

        var payload = { "AadharNo": aadharNo, "VoterId": voterId, "VoteFor": castVoteFor, "Otp": userEnteredOtp };

        $http({
            method: 'POST',
            url: 'api/Votes/CastVote',
            data: JSON.stringify(payload),
            headers: { 'Content-Type': 'application/json; charset=utf-8' }
        })
        .then(function (response) {
            if (response.data === "SuccessfullyVoted") {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", true, undefined, undefined);
                $scope.updateCastVoteElements("block", true, "Voting successful.", "#07bb07");
            }
            else if (response.data === "AlreadyVoted") {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", true, undefined, undefined);
                $scope.updateCastVoteElements("block", true, "You have already voted.", "red");
            }
            else if (response.data === "TooManyTries") {
                 setTimeout(function () {
                     // Retry after 2 seconds.
                     $http({
                         method: 'POST',
                         url: 'api/Votes/CastVote',
                         data: JSON.stringify(payload),
                         headers: { 'Content-Type': 'application/json; charset=utf-8' }
                     })
                     .then(function (response) {
                         if (response.data === "SuccessfullyVoted") {
                             $scope.updateAadharElements("block", true, undefined, undefined);
                             $scope.updateOtpElements("block", true, undefined, undefined);
                             $scope.updateVoterCardElements("block", true, undefined, undefined);
                             $scope.updateCastVoteElements("block", true, "Voting successful.", "#07bb07");
                         }
                         else if (response.data === "AlreadyVoted") {
                             $scope.updateAadharElements("block", true, undefined, undefined);
                             $scope.updateOtpElements("block", true, undefined, undefined);
                             $scope.updateVoterCardElements("block", true, undefined, undefined);
                             $scope.updateCastVoteElements("block", true, "You have already voted.", "red");
                         }
                         else {
                             $scope.updateAadharElements("block", true, undefined, undefined);
                             $scope.updateOtpElements("block", true, undefined, undefined);
                             $scope.updateVoterCardElements("block", true, undefined, undefined);
                             $scope.updateCastVoteElements("block", false, "Voting failed, please try again.", "red");
                         }
                     });
                 }, 2000);
            }
            else {
                $scope.updateAadharElements("block", true, undefined, undefined);
                $scope.updateOtpElements("block", true, undefined, undefined);
                $scope.updateVoterCardElements("block", true, undefined, undefined);
                $scope.updateCastVoteElements("block", false, "Voting failed, please try again.", "red");
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
            document.getElementById("txtCaptcha").disabled = disableAadharElements;
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

    // Update Vote UI elements.
    $scope.updateCastVoteElements = function (castVoteDivDisplay, disableCastVoteElements, castVoteMsg, castVoteColor) {
        if (castVoteDivDisplay !== undefined) {
            document.getElementById("CastVoteDiv").style.display = castVoteDivDisplay;
        }
        if (disableCastVoteElements !== undefined) {
            var candidateList = document.getElementsByClassName("candidates");
            for (var i = 0; i < candidateList.length; ++i) {
                candidateList[i].disabled = disableCastVoteElements;
            }
        }
        if (castVoteMsg !== undefined) {
            $scope.castVoteMessage = castVoteMsg;
            console.log(castVoteMsg);
        }
        if (castVoteColor !== undefined) {
            $scope.castVoteColor = castVoteColor;
        }
    };
}]);