/// <reference path="typings/lodash/lodash.d.ts" />


module NakedObjects {
    app.run((error: IError) => {



        // set as many display handlers as you want. They are called in order
        // if no template is set tyhen the default error template will be used. 
        error.setErrorDisplayHandler(($scope: INakedObjectsScope) => {
            if ($scope.error.isConcurrencyError) {
                $scope.errorTemplate = concurrencyTemplate;
            }
        });

    });
}