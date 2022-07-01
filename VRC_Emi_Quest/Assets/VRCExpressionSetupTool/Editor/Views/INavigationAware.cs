namespace VRCExpressionSetupTool.Editor.Views
{
    internal interface INavigationAware
    {
        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        void OnNavigatedTo();
        
        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        void OnNavigatedFrom();
    }
}