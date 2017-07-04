(function () {

    "use strict";

    // getting the existing module
    angular.module("app-trips")
	.controller("tripsController", tripsController);

    function tripsController($http) {

        var vm = this;

        vm.trips = [];

        vm.newTrip = {};
        vm.errorMessage = "";
        vm.successMessage = "";
        vm.isBusy = true;

        $http.get("/api/trips")
		.then(function (response) {
		    // success
		    angular.copy(response.data, vm.trips);
		}, function (error) {
		    // failure
		    vm.errorMessage = "Failed to load data: " + error;
		})
		.finally(function () {
		    vm.isBusy = false;
		});

        vm.addTrip = function () {

            vm.isBusy = true;
            vm.errorMessage = "";
            vm.successMessage = "";

            console.log(vm.newTrip);

            $http.post("/api/trips", vm.newTrip)
			.then(function (response) {
			    // success
			    vm.trips.push(response.data);
			    vm.successMessage = "Trip " + vm.newTrip.name + " added successfully!";
			    vm.newTrip = {};
			}, function () {
			    // failure
			    vm.errorMessage = "Failed to save new trip";
			})
			.finally(function () {
			    vm.isBusy = false;
			});
        };

        vm.deleteTrip = function (idx) {

            vm.errorMessage = "";
            vm.successMessage = "";

            var trip = vm.trips[idx];
            console.log(trip);
            var tripName = trip.name;

            console.log("tripName: " + tripName);
            vm.isBusy = true;

            $http.delete("/api/trips/" + tripName)
            .then(function () {
                // succcess
                var msg = "Trip" + tripName + " deleted successfully!"
                vm.successMessage = msg;

                vm.trips.splice(idx, 1);

            }, function (error) {
                // failure
                var msg = JSON.stringify(error);
                vm.errorMessage = "Failed to delete trip, error: " + msg;
            })
            .finally(function () {
                vm.isBusy = false;
            });
        };
    }

})();
