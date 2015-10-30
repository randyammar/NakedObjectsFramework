/// <reference path="typings/angularjs/angular.d.ts" />
/// <reference path="typings/lodash/lodash.d.ts" />
/// <reference path="nakedobjects.models.ts" />


module NakedObjects.Angular.Gemini {

    export interface IFocusManager {
        focusOn(name: string): void;
    }

    app.service("focusManager", function ($timeout, $rootScope) {
        const helper = <IFocusManager>this;

        helper.focusOn = (name: string) => {
            $timeout(function () {
                $rootScope.$broadcast('geminiFocuson', name);
            });
        }


    });
}