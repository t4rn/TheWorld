(function () {

    "use strict";

    angular.module("app-trips")
    .controller("tripEditorController", tripEditorController);

    function tripEditorController($routeParams, $http) {

        var vm = this;

        vm.tripName = $routeParams.tripName;
        vm.stops = [];
        vm.errorMessage = "";
        vm.isBusy = true;
        vm.newStop = {};

        var url = "/api/trips/" + vm.tripName + "/stops/";

        $http.get(url)
        .then(function (response) {
            // success
            angular.copy(response.data, vm.stops);
            _showMap(vm.stops);
        }, function (err) {
            // failure
            vm.errorMessage = "Failed to load stops: " + err;
        })
        .finally(function () {
            vm.isBusy = false;
        });

        vm.addStop = function () {

            vm.errorMessage = "";
            vm.isBusy = true;

            console.log(vm.newStop);

            $http.post(url, vm.newStop)
            .then(function (response) {
                // success
                vm.stops.push(response.data);
                _showMap(vm.stops);
                vm.newStop = {};

            }, function (err) {
                // failure
                vm.errorMessage = "Failed to add new stop: " + err;
            })
            .finally(function () {
                vm.isBusy = false;
            });
        };

        vm.deleteStop = function (idx) {

            vm.errorMessage = "";

            var stop = vm.stops[idx];
            console.log(stop);
            var stopName = stop.name;

            console.log("stopName: " + stopName);
            vm.isBusy = true;

            $http.delete(url + stopName)
            .then(function () {
                // succcess
                var msg = "Stop" + stopName + " deleted successfully!"
                console.log(msg);

                vm.stops.splice(idx, 1);

                // refresh map
                _showMap(vm.stops);

            }, function (error) {
                // failure
                var msg = JSON.stringify(error);
                vm.errorMessage = "Failed to delete stop, error: " + msg;
            })
            .finally(function () {
                vm.isBusy = false;
            });
        }

    }

    function _showMap(stops) {
        if (stops && stops.length > 0) {

            // underscore mapping
            var mapStops = _.map(stops, function (item) {
                return {
                    lat: item.latitude,
                    long: item.longitude,
                    info: item.name
                };
            });

            // show map
            travelMap.createMap({
                stops: mapStops,
                selector: "#map",
                currentStop: 1,
                initialZoom: 5
            });
        }
    }

})();