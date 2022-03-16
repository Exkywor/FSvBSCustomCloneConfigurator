// Code from https://github.com/ME3Tweaks/ME3TweaksModManager/

namespace MassEffectModManagerCore.modmanager.save.game2.FileFormats
{
    public interface IUnrealSerializable
    {
        void Serialize(IUnrealStream stream);
    }
}
