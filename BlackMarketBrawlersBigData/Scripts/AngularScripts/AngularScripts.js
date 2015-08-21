(function () {
    var app = angular.module("application", ['ui.bootstrap']);

    //directive for the champions to display them in table format
    app.directive('championTable', function () {
        return {
            restrict: 'E',
            templateUrl: '/HTML/championTable.html',
            controller: ['$scope', '$http', '$modal', function ($scope, $http, $modal) {
                //Variable declarations
                $scope.champions;
                $scope.championSet = {};
                $scope.highestKD = 0;
                $scope.highestGameCount = 0;
                $scope.highestKDChamp = {};
                $scope.highestGameCountChamp = {};
                $scope.topChampions = [];
                $scope.middleChampions = [];
                $scope.bottomChampions = [];
                $scope.jungleChampions = [];
                $scope.championNames = [];
                $scope.championNamesRows = [];
                $scope.activeTab = "champ";
                $scope.searchChampions = undefined;

                //function to change tab class
                $scope.setActiveTab = function (item) {
                    $scope.activeTab = item;
                };

                $scope.isActive = function (item) {
                    return (item === $scope.activeTab);
                };

                //ajax call to get all of the champions in Json form
                $scope.getChampions = function () {
                    var path = "/Home/getChampions";
                    $http.get(path).success(function (data) {
                        $scope.champions = angular.fromJson(data);
                        for (var i = 0, max = $scope.champions.length; i < max; i++) {
                            if ($scope.champions[i] !== undefined && $scope.champions[i] !== null) {
                                $scope.championNames.push($scope.champions[i].ChampionName);
                                $scope.champions[i] = $scope.calculateLanePreference($scope.champions[i]);
                                if ($scope.highestKD < $scope.champions[i].AverageKDA) {
                                    $scope.highestKD = $scope.champions[i].AverageKDA;
                                    $scope.highestKDChamp = $scope.champions[i];
                                }
                                if ($scope.highestGameCount < $scope.champions[i].gameCount) {
                                    $scope.highestGameCount = $scope.champions[i].gameCount;
                                    $scope.highestGameCountChamp = $scope.champions[i];
                                }
                            }
                        }
                        $scope.setChampionNamesRows();
                        $scope.calculateMostPlayedLane();
                    }).error(function (data) {
                        console.log(data);
                    });
                };

                //Gets 4 rows of champions names for graphs
                $scope.setChampionNamesRows = function () {
                    var temp = [], length = $scope.championNames.length + 1, j = 1;
                    for (; j < length; j++) {
                        if (j % 32 == 0) {
                            temp.push($scope.championNames[j]);
                            $scope.championNamesRows.push(temp);
                            temp = [];
                        } else {
                            temp.push($scope.championNames[j]);
                        }
                    }
                    $scope.championNamesRows.push(temp);
                    temp = [];
                };

                //set which champion was clicked on
                $scope.setChampion = function (champ) {
                    $scope.championSet = champ;
                };

                //open a modal based off of which champion was clicked on
                $scope.open = function (size) {

                    var modalInstance = $modal.open({
                        animation: $scope.animationsEnabled,
                        templateUrl: '/HTML/championModal.html',
                        controller: 'championModal',
                        size: size,
                        resolve: {
                            champions: function () {
                                return $scope.championSet;
                            }
                        }
                    });

                    modalInstance.result.then(function () {
                        $log.info('Modal dismissed at: ' + new Date());
                    });
                };

                //figure out which lane each champion prefers the most
                $scope.calculateLanePreference = function (champion) {
                    if (champion !== undefined && champion !== null) {
                        champion.LaneData = [champion.lanePrefCount["TOP"], champion.lanePrefCount["BOTTOM"], champion.lanePrefCount["MIDDLE"], champion.lanePrefCount["JUNGLE"]];
                        var i = 0, highestCount = 0, max = champion.LaneData.length, index = 0;
                        for (; i < max; i++) {
                            if (champion.LaneData[i] > highestCount) {
                                highestCount = champion.LaneData[i];
                                index = i;
                            }
                        }
                        switch (index) {
                            case 0: { champion.LanePref = "Top"; $scope.topChampions.push(champion); } break;
                            case 1: { champion.LanePref = "Bottom"; $scope.bottomChampions.push(champion); } break;
                            case 2: { champion.LanePref = "Middle"; $scope.middleChampions.push(champion); } break;
                            case 3: { champion.LanePref = "Jungle"; $scope.jungleChampions.push(champion); } break;
                        }
                    }
                    return champion;
                };

                $scope.getChampions();

            }]
        };

    });

    //Directive for all of the stats for the series of data and champions
    app.directive('statsTable', function () {
        return {
            restrict: 'E',
            templateUrl: '/HTML/statsTable.html',
            controller: ['$scope', function ($scope) {
                //variable declaration
                $scope.MostPlayedLane = "";
                $scope.isGraph1Complete = false;
                $scope.isGraph2Complete = false;
                $scope.isGraph3Complete = false;

                //Calculate which lane was played the most by a champion
                $scope.calculateMostPlayedLane = function () {
                    var array = [$scope.topChampions.length, $scope.bottomChampions.length, $scope.middleChampions.length, $scope.jungleChampions.length], temp = 0;
                    for (var i = 0; i < array.length; i++) {
                        if (temp < array[i]) {
                            temp = array[i];
                        }
                    }
                    switch (temp) {
                        case $scope.topChampions.length: { $scope.MostPlayedLane = "Top"; } break;
                        case $scope.bottomChampions.length: { $scope.MostPlayedLane = "Bottom"; } break;
                        case $scope.middleChampions.length: { $scope.MostPlayedLane = "Middle"; } break;
                        case $scope.jungleChampions.length: { $scope.MostPlayedLane = "Jungle"; } break;
                    }
                };

                //functions to draw bar graphs
                $scope.drawBarGraph = function (data1, data2, divName, canvasName, label1, label2, rowNumber) {
                    try {
                        var millisecondsToWait = 200;
                        setTimeout(function () {
                            var ctx = $("#" + canvasName).get(0);
                            ctx = ctx.getContext("2d");
                            var data = {
                                labels: $scope.championNamesRows[rowNumber],
                                datasets: [
                                    {
                                        label: label1,
                                        fillColor: "rgba(220,220,220,0.5)",
                                        strokeColor: "rgba(220,220,220,0.8)",
                                        highlightFill: "rgba(220,220,220,0.75)",
                                        highlightStroke: "rgba(220,220,220,1)",
                                        data: data1
                                    },

                                {
                                    label: label2,
                                    fillColor: "rgba(151,187,205,0.5)",
                                    strokeColor: "rgba(151,187,205,0.8)",
                                    highlightFill: "rgba(151,187,205,0.75)",
                                    highlightStroke: "rgba(151,187,205,1)",
                                    data: data2
                                }
                                ]
                            };
                            var myBarGraph = new Chart(ctx).Bar(data, {
                                legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>"
                            });
                            var millisecondsToWait = 200;
                            setTimeout(function () {
                                $("#" + divName).append(myBarGraph.generateLegend());
                            });
                        }, millisecondsToWait);
                    } catch (error) {
                        console.log(error.message);
                    }
                };

                $scope.drawLanePrefGraph = function () {
                    if (!$scope.isGraph1Complete) {
                        var data1 = [], data2 = [];
                        for (var i = 1, max = $scope.champions.length + 1, j = 0; i < max; i++) {
                            if ($scope.champions[i] !== undefined && $scope.champions[i] !== null) {
                                data1.push($scope.champions[i].gameCount);
                                data2.push($scope.champions[i].lanePrefCount[$scope.champions[i].LanePref.toUpperCase()]);
                            }
                            if (i % 32 == 0) {
                                $scope.drawBarGraph(data1, data2, 'lanePrefrencesDiv' + j, 'lanePrefrences' + j, 'Total Games', 'Lane Total', j);
                                j++;
                                data1 = [];
                                data2 = [];
                            }
                        }
                        $scope.drawBarGraph(data1, data2, 'lanePrefrencesDiv3', 'lanePrefrences3', 'Total Games', 'Lane Total', 3);
                        $scope.isGraph1Complete = true;
                    }
                };

                $scope.drawKDGraph = function () {
                    if (!$scope.isGraph2Complete) {
                        var data1 = [], data2 = [];
                        for (var i = 1, max = $scope.champions.length + 1, j = 0; i < max; i++) {
                            if ($scope.champions[i] !== undefined && $scope.champions[i] !== null) {
                                data1.push($scope.champions[i].totalKills);
                                data2.push($scope.champions[i].totalDeaths);
                            }
                            if (i % 32 == 0) {
                                $scope.drawBarGraph(data1, data2, 'kdStatsDiv' + j, 'kdStats' + j, 'Total Kills', 'Lane Deaths', j);
                                j++;
                                data1 = [];
                                data2 = [];
                            }
                        }
                        $scope.drawBarGraph(data1, data2, 'kdStatsDiv3', 'kdStats3', 'Total Kills', 'Lane Deaths', 3);
                        $scope.isGraph2Complete = true;
                    }
                };

                $scope.drawGameGraph = function () {
                    if (!$scope.isGraph3Complete) {
                        var data1 = [], data2 = [];
                        for (var i = 1, max = $scope.champions.length + 1, j = 0; i < max; i++) {
                            if ($scope.champions[i] !== undefined && $scope.champions[i] !== null) {
                                data1.push($scope.champions[i].gameCount);
                                data2.push($scope.champions[i].banCount);
                            }
                            if (i % 32 == 0) {
                                $scope.drawBarGraph(data1, data2, 'gameStatsDiv' + j, 'gameStats' + j, 'Total Games', 'Ban Count', j);
                                j++;
                                data1 = [];
                                data2 = [];
                            }
                        }
                        $scope.drawBarGraph(data1, data2, 'gameStatsDiv3', 'gameStats3', 'Total Games', 'Ban Count', 3);
                        $scope.isGraph3Complete = true;
                    }
                };

            }]
        };

    });

    //Modal for when user clicks on a champions photo
    app.controller('championModal', ['$scope', '$modalInstance', 'champions', function ($scope, $modalInstance, champions) {
        //Variable declarations
        $scope.champion = champions;
        $scope.myInterval = 6000;
        $scope.noWrapSlides = false;
        $scope.drewRadar = false;
        $scope.drewPie = false;
        $scope.drewPolar = false;
        $scope.drewDoghnut = false;
        $scope.Lanes = ["Top", "Bottom", "Middle", "Jungle"];
        $scope.championItems = $scope.champion.itemStats;

        //Carosel Slide functions from angularJS bootstrap-ui
        var slides = $scope.slides = [];
        $scope.addSlide = function (item) {
            var newWidth = 600 + slides.length + 1;
            slides.push({
                image: '/Images/ItemImages/' + item.itemName.replace(":", "") + ".png",
                text: 'Name: ' + item.itemName + ' ' +
                  'Used: ' + item.numberOfTimesUsed
            });
        };

        //Functions to draw all of the graphs from the champions data
        $scope.drawRadar = function () {
            try {
                if (!$scope.drewRadar) {
                    var millisecondsToWait = 500;
                    setTimeout(function () {
                        var ctx = $("#lanePrefRadar").get(0);
                        ctx = ctx.getContext("2d");
                        var data = {
                            labels: $scope.Lanes,
                            datasets: [
                                {
                                    label: "Lane Prefrences",
                                    fillColor: "rgba(220,220,220,0.2)",
                                    strokeColor: "rgba(220,220,220,1)",
                                    pointColor: "rgba(220,220,220,1)",
                                    pointStrokeColor: "#fff",
                                    pointHighlightFill: "#fff",
                                    pointHighlightStroke: "rgba(220,220,220,1)",
                                    data: $scope.champion.LaneData
                                }
                            ]
                        };

                        var myRadarChart = new Chart(ctx).Radar(data);
                        document.getElementById('lanePrefRadarLegend').innerHTML = myRadarChart.generateLegend();
                        $scope.drewRadar = true;
                    }, millisecondsToWait);
                }
            } catch (error) {
                console.log(error.message);
            }
        };

        $scope.drawPie = function () {
            try {
                if (!$scope.drewPie) {
                    var millisecondsToWait = 500;
                    setTimeout(function () {
                        var ctx = $("#killDeathStats").get(0);
                        ctx = ctx.getContext("2d");
                        var data = [
            {
                value: $scope.champion.totalDeaths,
                color: "#F7464A",
                highlight: "#FF5A5E",
                label: "Deaths"
            },
            {
                value: $scope.champion.totalKills,
                color: "#46BFBD",
                highlight: "#5AD3D1",
                label: "Kills"
            },
                        ]

                        var myPieChart = new Chart(ctx).Pie(data);
                        document.getElementById('killDeathStatsLegend').innerHTML = myPieChart.generateLegend();
                        $scope.drewPie = true;
                    }, millisecondsToWait);
                }
            } catch (error) {
                console.log(error.message);
            }
        };

        $scope.drawDoghnut = function () {
            try {
                if (!$scope.drewDoghnut) {
                    var millisecondsToWait = 500;
                    setTimeout(function () {
                        var ctx = $("#banStats").get(0);
                        ctx = ctx.getContext("2d");
                        var data = [
            {
                value: $scope.champion.gameCount,
                color: "#F7464A",
                highlight: "#FF5A5E",
                label: "Game Count"
            },
            {
                value: $scope.champion.banCount,
                color: "#46BFBD",
                highlight: "#5AD3D1",
                label: "Ban Count"
            },
                        ]



                        var myDoughnutChart = new Chart(ctx).Doughnut(data);
                        document.getElementById('banStatsLegend').innerHTML = myDoughnutChart.generateLegend();
                        $scope.drewDoghnut = true;
                    }, millisecondsToWait);
                }
            } catch (error) {
                console.log(error.message);
            }
        };

        $scope.drawPolarAreaChart = function () {
            try {
                if (!$scope.drewPolar) {
                    var millisecondsToWait = 500;
                    setTimeout(function () {
                        var ctx = $("#averageGameStats").get(0);
                        ctx = ctx.getContext("2d");
                        var data = [
            {
                value: $scope.champion.killRate,
                color: "#46BFBD",
                highlight: "#5AD3D1",
                label: "Kills Per Game"
            },
            {
                value: $scope.champion.deathRate,
                color: "#F7464A",
                highlight: "#FF5A5E",
                label: "Deaths Per Game"
            },
            {
                value: $scope.champion.AverageKDA,
                color: "#4D5360",
                highlight: "#616774",
                label: "Average KD"
            }
                        ]

                        var myPolarChart = new Chart(ctx).PolarArea(data);
                        document.getElementById('averageGameStatsLegend').innerHTML = myPolarChart.generateLegend();
                        $scope.drewPolar = true;
                    }, millisecondsToWait);
                }
            } catch (error) {
                console.log(error.message);
            }
        };

        //Gets the 8 most used items by a champion
        $scope.eightMostUsedItems = function () {
            var highestUsedItemCount = 0, highestItem = {}, itemArray = [];
            for (var i = 0, max = 7; i < max; i++) {
                for (item in $scope.championItems) {
                    try {
                        if (highestUsedItemCount < $scope.championItems[item].numberOfTimesUsed && itemArray.indexOf($scope.championItems[item]) === -1) {
                            highestItem = $scope.championItems[item];
                            highestUsedItemCount = $scope.championItems[item].numberOfTimesUsed;
                        }
                    } catch (error) {
                        console.log(error.message);
                    }
                }
                itemArray.push(highestItem);
                $scope.addSlide(highestItem);
                highestUsedItemCount = 0;
                highestItem = {};
            }
        };

        //closes Modal
        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };

        $scope.eightMostUsedItems();

    }]);

    //Directive to handle all of the data mining
    app.directive('loadingScreen', function () {
        return {
            restrict: 'E',
            templateUrl: '/HTML/loadingPage.html',
            controller: ['$scope', '$http', function ($scope, $http) {
                //varibale declaration
                $scope.doneDatamining = false;
                $scope.Message = "Data Mining is Fun!";

                //Ajax call to start data mining
                $scope.startDatamining = function () {
                    var path = "/Home/startDataMining";
                    $http.get(path).success(function (data) {
                        $scope.doneDatamining = true;
                        $scope.Message = data;
                    }).error(function (data) {
                        $scope.doneDatamining = true;
                        $scope.Message = data;
                    });
                };

                $scope.startDatamining();

            }]
        };

    });

})();