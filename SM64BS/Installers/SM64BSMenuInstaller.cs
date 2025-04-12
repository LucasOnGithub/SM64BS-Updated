using SM64BS.Managers;
using SM64BS.Patches;
using SM64BS.UI;
using Zenject;

namespace SM64BS.Installers
{
    internal class SM64BSMenuInstaller : Installer<SM64BSMenuInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<MenuMarioManager>().AsSingle();
            Container.BindInterfacesTo<MainMenuFlowPatch>().AsSingle();
            Container.Bind<SettingsViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}
