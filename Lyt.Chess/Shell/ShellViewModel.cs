namespace Lyt.Chess.Shell;

using static Messaging.ApplicationMessagingExtensions;

public sealed partial class ShellViewModel
    : ViewModel<ShellView>,
    IRecipient<ToolbarCommandMessage>,
    IRecipient<LanguageChangedMessage>
{
    private readonly ChessModel chessModel;
    private readonly Fullscreen fullscreen;
    private readonly IToaster toaster;

    [ObservableProperty]
    public bool mainToolbarIsVisible;

    private ViewSelector<ActivatedView>? viewSelector;
    public bool isFirstActivation;

    public ShellViewModel(ChessModel chessModel, IToaster toaster)
    {
        this.chessModel = chessModel;
        this.toaster = toaster;
        this.fullscreen = new Fullscreen(App.MainWindow);

        //this.Messenger.Subscribe<ViewActivationMessage>(this.OnViewActivation);
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<LanguageChangedMessage>();
    }

    public void Receive(LanguageChangedMessage _)
    {
    }

    public void Receive(ToolbarCommandMessage message) 
    { 
        if ( message.Command == ToolbarCommandMessage.ToolbarCommand.PlayFullscreen)
        {
            var vm = App.GetRequiredService<PlayViewModel>();
            this.fullscreen.GoFullscreen(this.View.ShellViewContent, vm.View);
        }
        else if (message.Command == ToolbarCommandMessage.ToolbarCommand.PlayWindowed)
        {
            this.fullscreen.ReturnToWindowed();
        }
    }

    public override void OnViewLoaded()
    {
        this.Logger.Debug("OnViewLoaded begins");

        base.OnViewLoaded();
        if (this.View is null)
        {
            throw new Exception("Failed to startup...");
        }

        // Select default language 
        string preferredLanguage = this.chessModel.Language;
        this.Logger.Debug("Language: " + preferredLanguage);
        this.Localizer.SelectLanguage(preferredLanguage);
        Thread.CurrentThread.CurrentCulture = new CultureInfo(preferredLanguage);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(preferredLanguage);

        this.Logger.Debug("OnViewLoaded language loaded");

        // Create all statics views and bind them 
        this.SetupWorkflow();
        this.Logger.Debug("OnViewLoaded SetupWorkflow complete");

        //// Ready 
        //this.toaster.Host = this.View.ToasterHost;
        //this.toaster.Show(
        //    this.Localize("Shell.Ready"), this.Localize("Shell.Greetings"),
        //    5_000, InformationLevel.Info);


        this.isFirstActivation = true;
        Select(ActivatedView.Play);
        //Select(this.jigsawModel.IsFirstRun ? ActivatedView.Language : ActivatedView.Setup);

        Task.Run(async () =>
        {
            bool ready = await this.chessModel.InitializeEngine();
            if (ready)
            {
                this.Logger.Debug("Engine ready");
                await Task.Delay(120);
                new ModelUpdatedMessage(UpdateHint.EngineReady, ready).Publish();
            }
            else
            {
                // TODO 
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        });

        this.Logger.Debug("OnViewLoaded complete");
    }

    private void SetupWorkflow()
    {
        if (this.View is not ShellView view)
        {
            throw new Exception("No view: Failed to startup...");
        }

        var selectableViews = new List<SelectableView<ActivatedView>>();

        void Setup<TViewModel, TControl, TToolbarViewModel, TToolbarControl>(
                ActivatedView activatedView, Control? control)
            where TViewModel : ViewModel<TControl>
            where TControl : Control, IView, new()
            where TToolbarViewModel : ViewModel<TToolbarControl>
            where TToolbarControl : Control, IView, new()
        {
            var vm = App.GetRequiredService<TViewModel>();
            vm.CreateViewAndBind();
            var vmToolbar = App.GetRequiredService<TToolbarViewModel>();
            vmToolbar.CreateViewAndBind();
            selectableViews.Add(
                new SelectableView<ActivatedView>(activatedView, vm, control, vmToolbar));
        }

        //void SetupNoToolbar<TViewModel, TControl>(
        //        ActivatedView activatedView, Control control)
        //    where TViewModel : ViewModel<TControl>
        //    where TControl : Control, IView, new()
        //{
        //    var vm = App.GetRequiredService<TViewModel>();
        //    vm.CreateViewAndBind();
        //    selectableViews.Add(new SelectableView<ActivatedView>(activatedView, vm));
        //}

        Setup<SetupViewModel, SetupView, SetupToolbarViewModel, SetupToolbarView>(
            ActivatedView.Setup, view.CollectionButton);

        Setup<PlayViewModel, PlayView, PlayToolbarViewModel, PlayToolbarView>(
            ActivatedView.Play, null);

        Setup<LanguageViewModel, LanguageView, LanguageToolbarViewModel, LanguageToolbarView>(
            ActivatedView.Language, view.FlagButton);

        Setup<IntroViewModel, IntroView, IntroToolbarViewModel, IntroToolbarView>(
            ActivatedView.Intro, view.IntroButton);

        // Needs to be kept alive as a class member, or else callbacks will die (and wont work) 
        this.viewSelector =
            new ViewSelector<ActivatedView>(
                this.View.ShellViewContent,
                this.View.ShellViewToolbar,
                this.View.SelectionGroup,
                selectableViews,
                this.OnViewSelected);

        // ViewSelector<ActivatedView>.Disable(ActivatedView.Play); 
    }

    private void OnViewSelected(ActivatedView activatedView)
    {
        if (this.viewSelector is null)
        {
            throw new Exception("No view selector");
        }

        var newViewModel = this.viewSelector.CurrentPrimaryViewModel;
        if (newViewModel is not null)
        {
            bool mainToolbarIsHidden = false;
            this.MainToolbarIsVisible = !mainToolbarIsHidden;
            this.Profiler.MemorySnapshot(
                newViewModel.ViewBase!.GetType().Name + ":  Activated", withGCCollect: false);
        }

        this.isFirstActivation = false;
    }

#pragma warning disable IDE0079 
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnNewGame()
    {
        Task.Run(async () =>
        {
            bool ready = await this.chessModel.InitializeEngine();
            if (ready)
            {

                new ModelUpdatedMessage(UpdateHint.EngineReady, ready).Publish();
                await Task.Delay(150);
                this.chessModel.NewGame(isPlayingWhite:false);
                this.chessModel.GameIsActive(isActive: true);
            }
            else
            {
                // TODO 
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        });
    }

    [RelayCommand]
    public void OnCollection() => Select(ActivatedView.Setup);

    [RelayCommand]
    public void OnInfo() => Select(ActivatedView.Intro);

    [RelayCommand]
    public void OnLanguage() => Select(ActivatedView.Language);

    [RelayCommand]
    public void OnClose() => OnExit();

    private static async void OnExit()
    {
        var application = App.GetRequiredService<IApplicationBase>();
        await application.Shutdown();
    }
#pragma warning restore CA1822
#pragma warning restore IDE0079
}
