/// <reference path="typings/angularjs/angular.d.ts" />
/// <reference path="nakedobjects.models.ts" />
/// <reference path="nakedobjects.userMessages.config.ts" />


module NakedObjects {

    import Value = Models.Value;
    import Link = Models.Link;
    import EntryType = Models.EntryType;
    import Parameter = Models.Parameter;
    import ActionMember = Models.ActionMember;
    import ActionResultRepresentation = Models.ActionResultRepresentation;
    import ErrorWrapper = Models.ErrorWrapper;
    import DomainObjectRepresentation = Models.DomainObjectRepresentation;
    import ErrorMap = Models.ErrorMap;
    import PropertyMember = Models.PropertyMember;
    import ListRepresentation = Models.ListRepresentation;
    import ErrorCategory = Models.ErrorCategory;
    import HttpStatusCode = Models.HttpStatusCode;
    import CollectionMember = Models.CollectionMember;
    import MenusRepresentation = Models.MenusRepresentation;
    import DateString = Models.toDateString;
    import ActionRepresentation = Models.ActionRepresentation;
    import IInvokableAction = Models.IInvokableAction;
    import CollectionRepresentation = Models.CollectionRepresentation;
    import scalarValueType = RoInterfaces.scalarValueType;
    import dirtyMarker = Models.dirtyMarker;
    import toTimeString = Models.toTimeString;
    import IVersionRepresentation = RoInterfaces.IVersionRepresentation;
    import IUserRepresentation = RoInterfaces.IUserRepresentation;
    import Extensions = Models.Extensions;

    function tooltip(onWhat: { clientValid: () => boolean }, fields: ValueViewModel[]): string {
        if (onWhat.clientValid()) {
            return "";
        }

        const missingMandatoryFields = _.filter(fields, p => !p.clientValid && !p.getMessage());

        if (missingMandatoryFields.length > 0) {
            return _.reduce(missingMandatoryFields, (s, t) => s + t.title + "; ", mandatoryFieldsPrefix);
        }

        const invalidFields = _.filter(fields, p => !p.clientValid);

        if (invalidFields.length > 0) {
            return _.reduce(invalidFields, (s, t) => s + t.title + "; ", invalidFieldsPrefix);
        }

        return "";
    }

    function actionsTooltip(onWhat: { disableActions: () => boolean }, actionsOpen: boolean) {
        if (actionsOpen) {
            return closeActions;
        }
        return onWhat.disableActions() ? noActions : openActions;
    }

    export function toTriStateBoolean(valueToSet: string | boolean | number) {

        // looks stupid but note type checking
        if (valueToSet === true || valueToSet === "true") {
            return true;
        }
        if (valueToSet === false || valueToSet === "false") {
            return false;
        }
        return null;
    }

    function getMenuForLevel(menupath: string, level: number) {
        let menu = "";

        if (menupath && menupath.length > 0) {
            const menus = menupath.split("_");

            if (menus.length > level) {
                menu = menus[level];
            }
        }

        return menu || "";
    }

    function removeDuplicateMenus(menus: MenuItemViewModel[]) {
        return _.uniqWith(menus, (a: MenuItemViewModel, b: MenuItemViewModel) => {
            if (a.name && b.name) {
                return a.name === b.name;
            }
            return false;
        });
    }

    export function createSubmenuItems(avms: ActionViewModel[], menu: MenuItemViewModel, level: number) {
        // if not root menu aggregate all actions with same name
        if (menu.name) {
            const actions = _.filter(avms, a => getMenuForLevel(a.menuPath, level) === menu.name &&
                !getMenuForLevel(a.menuPath, level + 1));
            menu.actions = actions;

            //then collate submenus 

            const submenuActions = _.filter(avms, a => getMenuForLevel(a.menuPath, level) === menu.name &&
                getMenuForLevel(a.menuPath, level + 1));
            let menus = _
                .chain(submenuActions)
                .map(a => new MenuItemViewModel(getMenuForLevel(a.menuPath, level + 1), null, null))
                .value();

            menus = removeDuplicateMenus(menus);

            menu.menuItems = _.map(menus, m => createSubmenuItems(submenuActions, m, level + 1));
        }
        return menu;
    }

    export function createMenuItems(avms: ActionViewModel[]) {

        // first create a top level menu for each action 
        // note at top level we leave 'un-menued' actions
        let menus = _
            .chain(avms)
            .map(a => new MenuItemViewModel(getMenuForLevel(a.menuPath, 0), [a], null))
            .value();

        // remove non unique submenus 
        menus = removeDuplicateMenus(menus);

        // update submenus with all actions under same submenu
        return _.map(menus, m => createSubmenuItems(avms, m, 0));
    }

    export class AttachmentViewModel implements IAttachmentViewModel {
        href: string;
        mimeType: string;
        title: string;
        link: Link;
        onPaneId: number;

        private parent: DomainObjectRepresentation;
        private context: IContext;

        static create(attachmentLink: Link, parent: DomainObjectRepresentation, context: IContext, paneId: number) {
            const attachmentViewModel = new AttachmentViewModel();
            attachmentViewModel.link = attachmentLink;
            attachmentViewModel.href = attachmentLink.href();
            attachmentViewModel.mimeType = attachmentLink.type().asString;
            attachmentViewModel.title = attachmentLink.title() || unknownFileTitle;
            attachmentViewModel.parent = parent;
            attachmentViewModel.context = context;
            attachmentViewModel.onPaneId = paneId;
            return attachmentViewModel as IAttachmentViewModel;
        }

        downloadFile = () => this.context.getFile(this.parent, this.href, this.mimeType);
        clearCachedFile = () => this.context.clearCachedFile(this.href);

        displayInline = () =>
            this.mimeType === "image/jpeg" ||
            this.mimeType === "image/gif" ||
            this.mimeType === "application/octet-stream";

        doClick: (right?: boolean) => void;
    }

    export class ChoiceViewModel implements IChoiceViewModel {
        name: string;

        private id: string;
        private search: string;
        private isEnum: boolean;
        private wrapped: Value;

        static create(value: Value, id: string, name?: string, searchTerm?: string) {
            const choiceViewModel = new ChoiceViewModel();
            choiceViewModel.wrapped = value;
            choiceViewModel.id = id;
            choiceViewModel.name = name || value.toString();
            choiceViewModel.search = searchTerm || choiceViewModel.name;

            choiceViewModel.isEnum = !value.isReference() && (choiceViewModel.name !== choiceViewModel.getValue().toValueString());
            return choiceViewModel as IChoiceViewModel;
        }

        getValue() {
            return this.wrapped;
        }

        equals(other: IChoiceViewModel) : boolean {
            return other instanceof ChoiceViewModel &&
                this.id === other.id &&
                this.name === other.name &&
                this.wrapped.toValueString() === other.wrapped.toValueString();
        }

        valuesEqual(other: IChoiceViewModel) : boolean {
           
            if (other instanceof ChoiceViewModel) {
                const thisValue = this.isEnum ? this.wrapped.toValueString().trim() : this.search.trim();
                const otherValue = this.isEnum ? other.wrapped.toValueString().trim() : other.search.trim();
                return thisValue === otherValue;
            }
            return false;
        }
    }

    export class ErrorViewModel implements IErrorViewModel{
        originalError : ErrorWrapper;
        title: string;
        message: string;
        stackTrace: string[];
        errorCode: string;
        description: string;
        isConcurrencyError: boolean;
    }

    export class LinkViewModel implements ILinkViewModel, IDraggableViewModel {
        // ILinkViewModel
        title: string;
        domainType: string;
        link: Link;

        doClick: (right?: boolean) => void;

        // IDraggableViewModel 
        color: string;
        value: scalarValueType;
        reference: string;
        choice: IChoiceViewModel;
        draggableType: string;
        canDropOn: (targetType: string) => ng.IPromise<boolean>;
    }

    export class ItemViewModel extends LinkViewModel implements IItemViewModel, IDraggableViewModel {
        tableRowViewModel: TableRowViewModel;
        selected: boolean;
        selectionChange: (index: number) => void;
    }

    export class RecentItemViewModel extends LinkViewModel implements IRecentItemViewModel, IDraggableViewModel {
        friendlyName: string;
    }

    abstract class MessageViewModel implements IMessageViewModel {
        private previousMessage = "";
        private message = "";
        
        clearMessage = () => {
            if (this.message === this.previousMessage) {
                this.resetMessage();
            } else {
                this.previousMessage = this.message;
            }
        }

        resetMessage = () => this.message = this.previousMessage = "";
        setMessage = (msg: string) => this.message = msg;
        getMessage = () => this.message; 
    }

    export class ValueViewModel extends MessageViewModel {

        constructor(ext: Extensions) {
            super();
            this.optional = ext.optional();
            this.description = ext.description();
            this.presentationHint = ext.presentationHint();
            this.mask = ext.mask();
            this.title = ext.friendlyName();
            this.returnType = ext.returnType();
            this.format = ext.format();
            this.multipleLines = ext.multipleLines() || 1;
            this.password = ext.dataType() === "password";                                         
        }

        id: string;
        argId: string;
        paneArgId: string;
        onPaneId: number;

        optional: boolean;
        description: string;
        presentationHint: string;
        mask: string;
        title: string;
        returnType: string;
        format: formatType;
        multipleLines: number;
        password: boolean;


        clientValid = true;
      

        type: "scalar" | "ref";
        reference: string = "";
        minLength: number;

        color: string;
        
        isCollectionContributed: boolean;
       
    
        arguments: _.Dictionary<Value>;
        
        
        currentValue: Value;
        originalValue: Value;

        localFilter: ILocalFilter;
        formattedValue: string;
        value: scalarValueType | Date;
      
        choices: IChoiceViewModel[] = [];  

        choice: IChoiceViewModel;
        multiChoices: IChoiceViewModel[];

        file: Link;     

        isDirty = () => false;

        entryType: EntryType;

        validate: (modelValue: any, viewValue: string, mandatoryOnly: boolean) => boolean;

        refresh: (newValue: Value) => void;

        prompt(searchTerm: string): ng.IPromise<ChoiceViewModel[]> {
            return null;
        }

        conditionalChoices(args: _.Dictionary<Value>): ng.IPromise<ChoiceViewModel[]> {
            return null;
        }

        setNewValue(newValue: IDraggableViewModel) {
            this.value = newValue.value;
            this.reference = newValue.reference;
            this.choice = newValue.choice;
            this.color = newValue.color;
        }

        drop: (newValue: IDraggableViewModel) => void;

        clear() {
            this.value = null;
            this.reference = "";
            this.choice = null;
            this.color = "";
        }

        setColor(color: IColor) {

            if (this.entryType === EntryType.AutoComplete && this.choice && this.type === "ref") {
                const href = this.choice.getValue().href();
                if (href) {
                    color.toColorNumberFromHref(href).then((c: number) => this.color = `${linkColor}${c}`);
                    return;
                }
            }
            else if (this.entryType !== EntryType.AutoComplete && this.value) {
                color.toColorNumberFromType(this.returnType).then((c: number) => this.color = `${linkColor}${c}`);
                return;
            }

            this.color = "";
        }

        getValue(): Value {

            if (this.entryType === EntryType.File) {
                return new Value(this.file);
            }

            if (this.entryType !== EntryType.FreeForm || this.isCollectionContributed) {

                if (this.entryType === EntryType.MultipleChoices || this.entryType === EntryType.MultipleConditionalChoices || this.isCollectionContributed) {
                    const selections = this.multiChoices || [];
                    if (this.type === "scalar") {
                        const selValues = _.map(selections, cvm => cvm.getValue().scalar());
                        return new Value(selValues);
                    }
                    const selRefs = _.map(selections, cvm => ({ href: cvm.getValue().href(), title: cvm.name })); // reference 
                    return new Value(selRefs);
                }

                const choiceValue = this.choice ? this.choice.getValue() : null;
                if (this.type === "scalar") {                
                    return new Value(choiceValue && choiceValue.scalar() != null ? choiceValue.scalar() : "");
                }

                // reference 
                return new Value(choiceValue && choiceValue.isReference() ? { href: choiceValue.href(), title: this.choice.name } : null);
            }

            if (this.type === "scalar") {
                if (this.value == null) {
                    return new Value("");
                }

                if (this.value instanceof Date) {

                    if (this.format === "time") {
                        // time format
                        return new Value(toTimeString(this.value as Date));
                    }

                    if (this.format === "date") {
                        // truncate time;
                        return new Value(DateString(this.value as Date));
                    }
                    // date-time
                    return new Value((this.value as Date).toISOString());
                }

                return new Value(this.value as scalarValueType);
            }

            // reference
            return new Value(this.reference ? { href: this.reference, title: this.value.toString() } : null);
        }
    }

    export class ParameterViewModel extends ValueViewModel {

        constructor(parmRep: Parameter, paneId : number) {
            super(parmRep.extensions());
            this.parameterRep = parmRep;
            this.onPaneId = paneId;
            this.type = parmRep.isScalar() ? "scalar" : "ref";
            this.dflt = parmRep.default().toString();
            this.id = parmRep.id();
            this.argId = `${this.id.toLowerCase()}`;
            this.paneArgId = `${this.argId}${this.onPaneId}`;
            this.isCollectionContributed = parmRep.isCollectionContributed();
            this.entryType = parmRep.entryType();
        }

        parameterRep: Parameter;
        dflt: string;
    }

    export class ActionViewModel {
        actionRep: ActionMember | ActionRepresentation;
        invokableActionRep: IInvokableAction;

        menuPath: string;
        title: string;
        description: string;
        presentationHint : string;

        // todo - confusing name better 
        doInvoke: (right?: boolean) => void;
        executeInvoke: (pps: ParameterViewModel[], right?: boolean) => ng.IPromise<ActionResultRepresentation>;

        disabled(): boolean { return false; }

        parameters: () => ParameterViewModel[];
        stopWatchingParms: () => void;

        makeInvokable: (details: IInvokableAction) => void;
    }

    export class MenuItemViewModel {

        constructor(public name: string,
            public actions: ActionViewModel[],
            public menuItems: MenuItemViewModel[]) { }

    }

    export class DialogViewModel extends MessageViewModel {
        constructor(private color: IColor,
            private context: IContext,
            private viewModelFactory: IViewModelFactory,
            private urlManager: IUrlManager,
            private focusManager: IFocusManager,
            private error: IError,
            private $rootScope: ng.IRootScopeService) {
            super();
        }

        reset(actionViewModel: ActionViewModel, routeData: PaneRouteData) {
            this.actionViewModel = actionViewModel;
            this.onPaneId = routeData.paneId;

            const fields = this.context.getCurrentDialogValues(this.actionMember().actionId(), this.onPaneId);

            const parameters = _.pickBy(actionViewModel.invokableActionRep.parameters(), p => !p.isCollectionContributed()) as _.Dictionary<Parameter>;
            this.parameters = _.map(parameters, p => this.viewModelFactory.parameterViewModel(p, fields[p.id()], this.onPaneId));

            this.title = this.actionMember().extensions().friendlyName();
            this.isQueryOnly = actionViewModel.invokableActionRep.invokeLink().method() === "GET";
            this.resetMessage();
            this.id = actionViewModel.actionRep.actionId();
            return this;
        }

        refresh() {
            const fields = this.context.getCurrentDialogValues(this.actionMember().actionId(), this.onPaneId);
            _.forEach(this.parameters, p => p.refresh(fields[p.id]));
        }


        private actionMember = () => this.actionViewModel.actionRep;
        title: string;

        isQueryOnly: boolean;
        onPaneId: number;
        id: string;

        deregister: () => void;

        actionViewModel: ActionViewModel;

        clientValid = () => _.every(this.parameters, p => p.clientValid);

        tooltip = () => tooltip(this, this.parameters);

        setParms = () => _.forEach(this.parameters, p => this.context.setFieldValue(this.actionMember().actionId(), p.parameterRep.id(), p.getValue(), this.onPaneId));

        private executeInvoke = (right?: boolean) => {

            const pps = this.parameters;
            _.forEach(pps, p => this.urlManager.setFieldValue(this.actionMember().actionId(), p.parameterRep, p.getValue(), this.onPaneId));
            this.context.updateValues();
            return this.actionViewModel.executeInvoke(pps, right);
        };

        doInvoke = (right?: boolean) =>
            this.executeInvoke(right).
                then((actionResult: ActionResultRepresentation) => {
                    if (actionResult.shouldExpectResult()) {
                        this.setMessage(actionResult.warningsOrMessages() || noResultMessage);
                    } else if (actionResult.resultType() === "void") {
                        // dialog staying on same page so treat as cancel 
                        // for url replacing purposes
                        this.doCloseReplaceHistory();
                    }
                    else if (!this.isQueryOnly) {
                        // not query only - always close
                        this.doCloseReplaceHistory();
                    }
                    else if (!right) {
                        // query only going to new page close dialog and keep history
                        this.doCloseKeepHistory();
                    }
                    // else query only going to other tab leave dialog open
                }).
                catch((reject: ErrorWrapper) => {
                    const display = (em: ErrorMap) => this.viewModelFactory.handleErrorResponse(em, this, this.parameters);
                    this.error.handleErrorAndDisplayMessages(reject, display);
                });

        doCloseKeepHistory = () => {
            this.deregister();
            this.urlManager.closeDialogKeepHistory(this.onPaneId);
        }

        doCloseReplaceHistory = () => {
            this.deregister();
            this.urlManager.closeDialogReplaceHistory(this.onPaneId);
        }

        clearMessages = () => {
            this.resetMessage();
            _.each(this.actionViewModel.parameters, parm => parm.clearMessage());
        };

        parameters: ParameterViewModel[];
    }

    export class PropertyViewModel extends ValueViewModel implements IDraggableViewModel {

        constructor(propertyRep: PropertyMember) {
            super(propertyRep.extensions());
            this.draggableType = propertyRep.extensions().returnType();

            this.propertyRep = propertyRep;
            this.entryType = propertyRep.entryType();
            this.isEditable = !propertyRep.disabledReason();
            this.entryType = propertyRep.entryType();
        }


        propertyRep: PropertyMember;
        target: string;
        isEditable: boolean;
        attachment: IAttachmentViewModel;
        draggableType: string;
        refType: "null" | "navigable" | "notNavigable";

        doClick(right?: boolean): void {}

        canDropOn: (targetType: string) => ng.IPromise<boolean>;

   
   }

    export class CollectionPlaceholderViewModel {
        description: () => string;
        reload: () => void;
    }

    export class ListViewModel extends MessageViewModel {

        constructor(private colorService: IColor,
            private context: IContext,
            private viewModelFactory: IViewModelFactory,
            private urlManager: IUrlManager,
            private focusManager: IFocusManager,
            private error: IError,
            private $q: ng.IQService) {
            super();
        }

        updateItems(value: Link[]) {
            this.items = this.viewModelFactory.getItems(value,
                this.state === CollectionViewState.Table,
                this.routeData,
                this);

            const totalCount = this.listRep.pagination().totalCount;
            this.allSelected = _.every(this.items, item => item.selected);
            const count = this.items.length;
            this.size = count;
            if (count > 0) {
                this.description = () => pageMessage(this.page, this.numPages, count, totalCount);
            } else {
                this.description = () => noItemsFound;
            }
        }

        hasTableData = () => {
            const valueLinks = this.listRep.value();
            return valueLinks && _.some(valueLinks, (i: Link) => i.members());
        }

        refresh(routeData: PaneRouteData) {

            this.routeData = routeData;
            if (this.state !== routeData.state) {
                this.state = routeData.state;
                if (this.state === CollectionViewState.Table && !this.hasTableData()) {
                    this.recreate(this.page, this.pageSize).
                        then(list => {
                            this.listRep = list;
                            this.updateItems(list.value());
                        }).
                        catch((reject: ErrorWrapper) => {
                            this.error.handleError(reject);
                        });
                } else {
                    this.updateItems(this.listRep.value());
                }
            }
        }

        collectionContributedActionDecorator(actionViewModel: ActionViewModel) {
            const wrappedInvoke = actionViewModel.executeInvoke;
            actionViewModel.executeInvoke = (pps: ParameterViewModel[], right?: boolean) => {
                const selected = _.filter(this.items, i => i.selected);

                if (selected.length === 0) {

                    const em = new ErrorMap({}, 0, noItemsSelected);
                    const rp = new ErrorWrapper(ErrorCategory.HttpClientError, HttpStatusCode.UnprocessableEntity, em);

                    return this.$q.reject(rp);
                }

                const getParms = (action: IInvokableAction) => {

                    const parms = _.values(action.parameters()) as Parameter[];
                    const contribParm = _.find(parms, p => p.isCollectionContributed());
                    const parmValue = new Value(_.map(selected, i => i.link));
                    const collectionParmVm = this.viewModelFactory.parameterViewModel(contribParm, parmValue, this.onPaneId);

                    const allpps = _.clone(pps);
                    allpps.push(collectionParmVm);
                    return allpps;
                }

                if (actionViewModel.invokableActionRep) {
                    return wrappedInvoke(getParms(actionViewModel.invokableActionRep), right);
                }

                return this.context.getActionDetails(actionViewModel.actionRep as ActionMember)
                    .then((details: ActionRepresentation) => wrappedInvoke(getParms(details), right));
            }
        }

        collectionContributedInvokeDecorator(actionViewModel: ActionViewModel) {
            const showDialog = () => this.context.getInvokableAction(actionViewModel.actionRep as ActionMember).
                then((ia: IInvokableAction) => _.keys(ia.parameters()).length > 1);

            actionViewModel.doInvoke = () => { };
            showDialog().
                then((show: boolean) => actionViewModel.doInvoke = show ?
                    (right?: boolean) => {
                        this.context.clearDialogValues(this.onPaneId);
                        this.focusManager.focusOverrideOff();
                        this.urlManager.setDialog(actionViewModel.actionRep.actionId(), this.onPaneId);
                    } :
                    (right?: boolean) => {
                        actionViewModel.executeInvoke([], right).
                            then(result => this.setMessage(result.shouldExpectResult() ? result.warningsOrMessages() || noResultMessage : "")).
                            catch((reject: ErrorWrapper) => {
                                const display = (em: ErrorMap) => this.setMessage(em.invalidReason() || em.warningMessage);
                                this.error.handleErrorAndDisplayMessages(reject,  display);
                            });
                    });
        }

        decorate(actionViewModel: ActionViewModel) {
            this.collectionContributedActionDecorator(actionViewModel);
            this.collectionContributedInvokeDecorator(actionViewModel);
        }

        reset(list: ListRepresentation, routeData: PaneRouteData) {
            this.listRep = list;
            this.routeData = routeData;

            this.id = this.urlManager.getListCacheIndex(routeData.paneId, routeData.page, routeData.pageSize);

            this.onPaneId = routeData.paneId;

            this.pluralName = "Objects";
            this.page = this.listRep.pagination().page;
            this.pageSize = this.listRep.pagination().pageSize;
            this.numPages = this.listRep.pagination().numPages;

            this.state = routeData.state;
            this.updateItems(list.value());

            const actions = this.listRep.actionMembers();
            this.actions = _.map(actions, action => this.viewModelFactory.actionViewModel(action, this, routeData));
            this.menuItems = createMenuItems(this.actions);

            _.forEach(this.actions, a => this.decorate(a));

            return this;
        }

        toggleActionMenu = () => {
            this.focusManager.focusOverrideOff();
            this.urlManager.toggleObjectMenu(this.onPaneId);
        };

        private recreate = (page: number, pageSize: number) => {
            return this.routeData.objectId ?
                this.context.getListFromObject(this.routeData.paneId, this.routeData, page, pageSize) :
                this.context.getListFromMenu(this.routeData.paneId, this.routeData, page, pageSize);
        };

        private pageOrRecreate = (newPage: number, newPageSize: number, newState?: CollectionViewState) => {
            this.recreate(newPage, newPageSize).
                then((list: ListRepresentation) => {
                    this.urlManager.setListPaging(newPage, newPageSize, newState || this.routeData.state, this.onPaneId);
                    this.routeData = this.urlManager.getRouteData().pane()[this.onPaneId];
                    this.reset(list, this.routeData);
                }).
                catch((reject: ErrorWrapper) => {
                    const display = (em: ErrorMap) => this.setMessage(em.invalidReason() || em.warningMessage);
                    this.error.handleErrorAndDisplayMessages(reject, display);
                });
        };

        listRep: ListRepresentation;
        routeData: PaneRouteData;

        size: number;
        pluralName: string;
        color: string;
        items: IItemViewModel[];
        header: string[];
        onPaneId: number;
        page: number;
        pageSize: number;
        numPages: number;
        state: CollectionViewState;

        allSelected: boolean;

        id: string;

        private setPage = (newPage: number, newState: CollectionViewState) => {
            this.focusManager.focusOverrideOff();
            this.pageOrRecreate(newPage, this.pageSize, newState);
        };
        pageNext = () => this.setPage(this.page < this.numPages ? this.page + 1 : this.page, this.state);
        pagePrevious = () => this.setPage(this.page > 1 ? this.page - 1 : this.page, this.state);
        pageFirst = () => this.setPage(1, this.state);
        pageLast = () => this.setPage(this.numPages, this.state);

        private earlierDisabled = () => this.page === 1 || this.numPages === 1;
        private laterDisabled = () => this.page === this.numPages || this.numPages === 1;

        private pageFirstDisabled = this.earlierDisabled;
        private pageLastDisabled = this.laterDisabled;
        private pageNextDisabled = this.laterDisabled;
        private pagePreviousDisabled = this.earlierDisabled;

        doSummary = () => {
            this.context.updateValues();
            this.urlManager.setListState(CollectionViewState.Summary, this.onPaneId);
        };
        doList = () => {
            this.context.updateValues();
            this.urlManager.setListState(CollectionViewState.List, this.onPaneId);
        };
        doTable = () => {
            this.context.updateValues();
            this.urlManager.setListState(CollectionViewState.Table, this.onPaneId);
        };

        reload = () => {
            this.context.clearCachedList(this.onPaneId, this.routeData.page, this.routeData.pageSize);
            this.setPage(this.page, this.state);
        };

        selectAll = () => _.each(this.items, (item, i) => {
            item.selected = this.allSelected;
            item.selectionChange(i);
        });

        description(): string { return null; } 

        template: string;

        disableActions(): boolean {
            return !this.actions || this.actions.length === 0 || !this.items || this.items.length === 0;
        }

        actionsTooltip = () => actionsTooltip(this, !!this.routeData.actionsOpen);

        actions: ActionViewModel[];
        menuItems: MenuItemViewModel[];

        actionMember = (id: string) => {
            const actionViewModel = _.find(this.actions, a => a.actionRep.actionId() === id);
            return actionViewModel.actionRep;
        }
    }

    export class CollectionViewModel {

        title: string;
        details: string;
        pluralName: string;
        color: string;
        mayHaveItems: boolean;
        items: IItemViewModel[];
        header: string[];
        onPaneId: number;
        currentState: CollectionViewState;
        presentationHint: string;

        id: string;

        doSummary(): void { }
        doTable(): void { }
        doList(): void { }

        description(): string { return this.details.toString() }

        template: string;

        actions: ActionViewModel[];
        menuItems: MenuItemViewModel[];
        messages: string;

        collectionRep: CollectionMember | CollectionRepresentation;
        refresh: (routeData: PaneRouteData, resetting: boolean) => void;
    }

    export class ServicesViewModel {
        title: string;
        color: string;
        items: ILinkViewModel[];
    }

    export class MenusViewModel {
        constructor(private viewModelFactory: IViewModelFactory) {

        }

        reset(menusRep: MenusRepresentation, routeData: PaneRouteData) {
            this.menusRep = menusRep;
            this.onPaneId = routeData.paneId;

            this.title = "Menus";
            this.color = "bg-color-darkBlue";
            this.items = _.map(this.menusRep.value(), link => this.viewModelFactory.linkViewModel(link, this.onPaneId));
            return this;
        }

        menusRep: MenusRepresentation;
        onPaneId: number;
        title: string;
        color: string;
        items: ILinkViewModel[];
    }

    export class ServiceViewModel extends MessageViewModel {
        title: string;
        serviceId: string;
        actions: ActionViewModel[];
        menuItems: MenuItemViewModel[];
        color: string;
    }

    export class MenuViewModel extends MessageViewModel {
        id: string;
        title: string;
        actions: ActionViewModel[];
        menuItems: MenuItemViewModel[];
        color: string;
        menuRep: Models.MenuRepresentation;
    }

    export class TableRowColumnViewModel {
        type: "ref" | "scalar";
        returnType: string;
        value: scalarValueType | Date;
        formattedValue: string;
        title: string;
    }

    export class TableRowViewModel {
        title: string;
        hasTitle: boolean;
        properties: TableRowColumnViewModel[];
    }

    export class DomainObjectViewModel extends MessageViewModel implements IDraggableViewModel {

        constructor(private colorService: IColor,
            private contextService: IContext,
            private viewModelFactory: IViewModelFactory,
            private urlManager: IUrlManager,
            private focusManager: IFocusManager,
            private error: IError,
            private $q: ng.IQService) {
            super();
        }

        propertyMap = () => {
            const pps = _.filter(this.properties, property => property.isEditable);
            return _.zipObject(_.map(pps, p => p.id), _.map(pps, p => p.getValue())) as _.Dictionary<Value>;
        };

        wrapAction(a: ActionViewModel) {
            const wrappedInvoke = a.executeInvoke;
            a.executeInvoke = (pps: ParameterViewModel[], right?: boolean) => {
                this.setProperties();
                const pairs = _.map(this.editProperties(), p => [p.id, p.getValue()]);
                const prps = (<any>_).fromPairs(pairs) as _.Dictionary<Value>;

                const parmValueMap = _.mapValues(a.invokableActionRep.parameters(), p => ({ parm: p, value: prps[p.id()] }));
                const allpps = _.map(parmValueMap, o => this.viewModelFactory.parameterViewModel(o.parm, o.value, this.onPaneId));
                return wrappedInvoke(allpps, right).
                    catch((reject: ErrorWrapper) => {
                        this.handleWrappedError(reject);
                        return this.$q.reject(reject);
                    });
            };
        }

        // must be careful with this - OK for changes on client but after server updates should use  reset
        // because parameters may have appeared or disappeared etc and refesh just updates existing views. 
        // So OK for view state changes but not eg for a parameter that disappears after saving

        refresh(routeData: PaneRouteData) {

            this.routeData = routeData;
            const iMode = this.domainObject.extensions().interactionMode();
            this.isInEdit = routeData.interactionMode !== InteractionMode.View || iMode === "form" || iMode === "transient";
            this.props = routeData.interactionMode !== InteractionMode.View ? this.contextService.getCurrentObjectValues(this.domainObject.id(), routeData.paneId) : {};

            _.forEach(this.properties, p => p.refresh(this.props[p.id]));
            _.forEach(this.collections, c => c.refresh(this.routeData, false));

            this.unsaved = routeData.interactionMode === InteractionMode.Transient;

            this.title = this.unsaved ? `Unsaved ${this.domainObject.extensions().friendlyName()}` : this.domainObject.title();

            this.title = this.title + dirtyMarker(this.contextService, this.domainObject.getOid());

            if (routeData.interactionMode === InteractionMode.Form) {
                _.forEach(this.actions, a => this.wrapAction(a));
            }

            // leave message from previous refresh 
            this.clearMessage();

            return this;
        }

        reset(obj: DomainObjectRepresentation, routeData: PaneRouteData) {
            this.domainObject = obj;
            this.onPaneId = routeData.paneId;
            this.routeData = routeData;
            const iMode = this.domainObject.extensions().interactionMode();
            this.isInEdit = routeData.interactionMode !== InteractionMode.View || iMode === "form" || iMode === "transient";
            this.props = routeData.interactionMode !== InteractionMode.View ? this.contextService.getCurrentObjectValues(this.domainObject.id(), routeData.paneId) : {};

            const actions = _.values(this.domainObject.actionMembers()) as ActionMember[];
            this.actions = _.map(actions, action => this.viewModelFactory.actionViewModel(action, this, this.routeData));

            this.menuItems = createMenuItems(this.actions);

            this.properties = _.map(this.domainObject.propertyMembers(), (property, id) => this.viewModelFactory.propertyViewModel(property, id, this.props[id], this.onPaneId, this.propertyMap));
            this.collections = _.map(this.domainObject.collectionMembers(), collection => this.viewModelFactory.collectionViewModel(collection, this.routeData));

            this.unsaved = routeData.interactionMode === InteractionMode.Transient;

            this.title = this.unsaved ? `Unsaved ${this.domainObject.extensions().friendlyName()}` : this.domainObject.title();

            this.title = this.title + dirtyMarker(this.contextService, obj.getOid());

            this.friendlyName = this.domainObject.extensions().friendlyName();
            this.presentationHint = this.domainObject.extensions().presentationHint();
            this.domainType = this.domainObject.domainType();
            this.instanceId = this.domainObject.instanceId();
            this.draggableType = this.domainObject.domainType();

            const selfAsValue = () => {
                const link = this.domainObject.selfLink();
                if (link) {
                    // not transient - can't drag transients so no need to set up IDraggable members on transients
                    link.setTitle(this.title);
                    return new Value(link);
                }
                return null;
            };
            const sav = selfAsValue();

            this.value = sav ? sav.toString() : "";
            this.reference = sav ? sav.toValueString() : "";
            this.choice = sav ? ChoiceViewModel.create(sav, "") : null;

            this.colorService.toColorNumberFromType(this.domainObject.domainType()).then((c: number) => {
                this.color = `${objectColor}${c}`;
            });

            this.resetMessage();

            if (routeData.interactionMode === InteractionMode.Form) {
                _.forEach(this.actions, a => this.wrapAction(a));
            }

            return this;
        }

        concurrency() {
            return (event: ng.IAngularEvent, em: ErrorMap) => {
                this.routeData = this.urlManager.getRouteData().pane()[this.onPaneId];
                this.contextService.getObject(this.onPaneId, this.domainObject.getOid(), this.routeData.interactionMode)
                    .then(obj => {
                        this.contextService.reloadObject(this.onPaneId, obj)
                            .then(reloadedObj => {
                                if (this.routeData.dialogId) {
                                    this.urlManager.closeDialogReplaceHistory(this.onPaneId);
                                }
                                this.reset(reloadedObj, this.routeData);
                                this.viewModelFactory.handleErrorResponse(em, this, this.properties);
                            });
                    });
            }
        }

        routeData: PaneRouteData;
        domainObject: DomainObjectRepresentation;
        onPaneId: number;
        props: _.Dictionary<Value>;

        title: string;
        friendlyName: string;
        presentationHint: string;
        domainType: string;
        instanceId: string;
        draggableType: string;
        isInEdit: boolean;
        value: string;
        reference: string;
        choice: IChoiceViewModel;
        color: string;
        actions: ActionViewModel[];
        menuItems: MenuItemViewModel[];
        properties: PropertyViewModel[];
        collections: CollectionViewModel[];
        unsaved: boolean;

        clientValid = () => _.every(this.properties, p => p.clientValid);

        tooltip = () => tooltip(this, this.properties);

        actionsTooltip = () => actionsTooltip(this, !!this.routeData.actionsOpen);

        toggleActionMenu = () => {
            this.focusManager.focusOverrideOff();
            this.urlManager.toggleObjectMenu(this.onPaneId);
        };

        private editProperties = () => _.filter(this.properties, p => p.isEditable && p.isDirty());

        setProperties = () =>
            _.forEach(this.editProperties(), p => this.contextService.setPropertyValue(this.domainObject, p.propertyRep, p.getValue(), this.onPaneId));

        private cancelHandler = () => this.domainObject.extensions().interactionMode() === "form" || this.domainObject.extensions().interactionMode() === "transient" ?
            () => this.urlManager.popUrlState(this.onPaneId) :
            () => this.urlManager.setInteractionMode(InteractionMode.View, this.onPaneId);

        editComplete = () => {
            this.setProperties();
            this.contextService.clearObjectUpdater(this.onPaneId);
        };

        doEditCancel = () => {
            this.editComplete();
            this.contextService.clearObjectValues(this.onPaneId);
            this.cancelHandler()();
        };

        clearCachedFiles = () => {
            _.forEach(this.properties, p => p.attachment ? p.attachment.clearCachedFile() : null);
        }

        private saveHandler = () => this.domainObject.isTransient() ? this.contextService.saveObject : this.contextService.updateObject;

        private validateHandler = () => this.domainObject.isTransient() ? this.contextService.validateSaveObject : this.contextService.validateUpdateObject;

        private handleWrappedError = (reject: ErrorWrapper) => {
            const display = (em: ErrorMap) => this.viewModelFactory.handleErrorResponse(em, this, this.properties);
            this.error.handleErrorAndDisplayMessages(reject, display);
        };

        doSave = (viewObject: boolean) => {
            this.clearCachedFiles();
            this.setProperties();
            const propMap = this.propertyMap();
            this.contextService.clearObjectUpdater(this.onPaneId);
            this.saveHandler()(this.domainObject, propMap, this.onPaneId, viewObject).
                then(obj => this.reset(obj, this.urlManager.getRouteData().pane()[this.onPaneId])).
                catch((reject: ErrorWrapper) => this.handleWrappedError(reject));
        };

        doSaveValidate = () => {
            const propMap = this.propertyMap();

            return this.validateHandler()(this.domainObject, propMap).
                then(() => {
                    this.resetMessage();
                    return true;
                }).
                catch((reject: ErrorWrapper) => {
                    this.handleWrappedError(reject);
                    return this.$q.reject(false);
                });
        };

        doEdit = () => {
            this.clearCachedFiles();
            this.contextService.clearObjectValues(this.onPaneId);
            this.contextService.getObjectForEdit(this.onPaneId, this.domainObject).
                then((updatedObject: DomainObjectRepresentation) => {
                    this.reset(updatedObject, this.urlManager.getRouteData().pane()[this.onPaneId]);
                    this.urlManager.pushUrlState(this.onPaneId);
                    this.urlManager.setInteractionMode(InteractionMode.Edit, this.onPaneId);
                }).
                catch((reject: ErrorWrapper) => this.handleWrappedError(reject));
        };

        doReload = () => {
            this.clearCachedFiles();
            this.contextService.reloadObject(this.onPaneId, this.domainObject)
                .then((updatedObject: DomainObjectRepresentation) => {
                    this.reset(updatedObject, this.urlManager.getRouteData().pane()[this.onPaneId]);
                })
                .catch((reject: ErrorWrapper) => this.handleWrappedError(reject));
        }


        hideEdit = () => this.domainObject.extensions().interactionMode() === "form" ||
            this.domainObject.extensions().interactionMode() === "transient" ||
            _.every(this.properties, p => !p.isEditable);

        disableActions(): boolean {
            return !this.actions || this.actions.length === 0;
        }

        canDropOn = (targetType: string) => this.contextService.isSubTypeOf(this.domainType, targetType);
    }


    export class ApplicationPropertiesViewModel {
        serverVersion: IVersionRepresentation;
        user: IUserRepresentation;
        serverUrl: string;
        clientVersion: string;
    }

    export class ToolBarViewModel {
        loading: string;
        template: string;
        footerTemplate: string;
        goHome: (right?: boolean) => void;
        goBack: () => void;
        goForward: () => void;
        swapPanes: () => void;
        logOff: () => void;
        singlePane: (right?: boolean) => void;
        recent: (right?: boolean) => void;
        cicero: () => void;
        userName: string;
        applicationProperties: () => void;

        warnings: string[];
        messages: string[];
    }

    export class RecentItemsViewModel {
        onPaneId: number;
        items: IRecentItemViewModel[];
    }

    export interface INakedObjectsScope extends ng.IScope {
        backgroundColor: string;
        title: string;

        homeTemplate: string;
        actionsTemplate: string;
        dialogTemplate: string;
        errorTemplate: string;
        listTemplate: string;
        recentTemplate: string;
        objectTemplate: string;
        collectionsTemplate: string;
        attachmentTemplate: string;
        applicationPropertiesTemplate: string;

        menus: MenusViewModel;
        object: DomainObjectViewModel;
        menu: MenuViewModel;
        dialog: DialogViewModel;
        error: ErrorViewModel;
        recent: RecentItemsViewModel;
        collection: ListViewModel;
        collectionPlaceholder: CollectionPlaceholderViewModel;
        toolBar: ToolBarViewModel;
        cicero: CiceroViewModel;
        attachment: IAttachmentViewModel;
        applicationProperties: ApplicationPropertiesViewModel;
    }

    export class CiceroViewModel {
        message: string;
        output: string;
        alert = ""; //Alert is appended before the output
        input: string;
        parseInput: (input: string) => void;
        previousInput: string;
        chainedCommands: string[];

        selectPreviousInput(): void {
            this.input = this.previousInput;
        }

        clearInput(): void {
            this.input = null;
        }

        autoComplete: (input: string) => void;

        outputMessageThenClearIt() {
            this.output = this.message;
            this.message = null;
        }

        renderHome: (routeData: PaneRouteData) => void;
        renderObject: (routeData: PaneRouteData) => void;
        renderList: (routeData: PaneRouteData) => void;
        renderError: () => void;
        viewType: ViewType;
        clipboard: DomainObjectRepresentation;

        executeNextChainedCommandIfAny: () => void;

        popNextCommand(): string {
            if (this.chainedCommands) {
                const next = this.chainedCommands[0];
                this.chainedCommands.splice(0, 1);
                return next;

            }
            return null;
        }

        clearInputRenderOutputAndAppendAlertIfAny(output: string): void {
            this.clearInput();
            this.output = output;
            if (this.alert) {
                this.output += this.alert;
                this.alert = "";
            }
        }
    }
}