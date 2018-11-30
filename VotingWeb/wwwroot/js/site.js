var app = angular.module('VotingApp', ['ui.bootstrap']);
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
        $scope.update("none");
        if (aadharNo === undefined || aadharNo.toString().length !== 12) {
            alert("Invalid Aadhar No");
            return;
        }
        $http.post('api/Votes/SubmitAadharNoToSendOtp/' + aadharNo, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .then(function (data) {
            // Use status and data to notify user accordingly.
            $scope.item = undefined;
            $scope.update("block");
            if (data.statusText === "OK") {
                $scope.OtpSendingMessage = "OTP sent to registered mobile no and email id.";
                $scope.OtpSendingStatus = "green";
            } else {
                $scope.OtpSendingMessage = "OTP sending to registered mobile no and email id failed, please try again.";
                $scope.OtpSendingStatus = "red";
            }
        });
    };

    $scope.update = function (enterOtpDivDisplay) {
        if (enterOtpDivDisplay !== undefined) {
            document.getElementById("txtEnterOtpDiv").style.display = enterOtpDivDisplay;
        }
    };
}]);