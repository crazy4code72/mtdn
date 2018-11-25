﻿var app = angular.module('VotingApp', ['ui.bootstrap']);
app.run(function () { });

app.controller('VotingAppController', ['$rootScope', '$scope', '$http', '$timeout', function ($rootScope, $scope, $http, $timeout) {

    $scope.refresh = function () {
        $http.get('api/Votes?c=' + new Date().getTime())
            .then(function (data, status) {
                $scope.votes = data;
            }, function (data, status) {
                $scope.votes = undefined;
            });
    };

    $scope.remove = function (item) {
        $http.delete('api/Votes/' + item)
            .then(function(data, status) {
                $scope.refresh();
            });
    };

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

    $scope.SubmitAadharNoToSendOtp = function (aadharNo) {
        if (aadharNo.toString().length !== 12) {
            alert("Invalid Aadhar No");
            return;
        }
        $http.post('api/Votes/SubmitAadharNoToSendOtp/' + aadharNo, {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
        })
        .then(function (data, status) {
            // Use status and data to notify user accordingly.
            $scope.item = undefined;
            if (data.statusText === "OK") {
                $scope.OtpSendStatus = "Otp sent successfully.";
            } else {
                $scope.OtpSendStatus = "Otp sending failed.";
            }
        });
    };
}]);