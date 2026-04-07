namespace SackranyUI.Core.Static
{
    public static class UIDependencyInjector
    {
        public static bool Inject(object viewModel, object template = null)
        {
            var ret = true;
            ret &= InjectTemplate(viewModel, template);
            return ret;
        }

        static bool InjectTemplate(object viewModel, object template)
        {
            if (template == null) return false;
            var meta = ViewModelReflectionCache.GetViewModelMetadata(viewModel.GetType());
            if (meta.Template.Field == null) return false;
            if (meta.Template.Type == template.GetType())
                meta.Template.Field.SetValue(viewModel, template);
            return true;
        }
    }
}