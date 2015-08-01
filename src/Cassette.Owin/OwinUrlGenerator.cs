using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cassette.Owin
{
    public class OwinUrlGenerator : IUrlGenerator
    {
        private readonly IUrlGenerator original;

        public OwinUrlGenerator(IUrlGenerator original)
        {
            if (original == null) throw new ArgumentNullException("original");

            this.original = original;
        }

        public string CreateAbsolutePathUrl(string applicationRelativePath)
        {
            return original.CreateAbsolutePathUrl(applicationRelativePath);
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return original.CreateAssetUrl(asset);
        }

        public string CreateBundleUrl(Bundle bundle)
        {
            return original.CreateBundleUrl(bundle);
        }

        public string CreateCachedFileUrl(string filename)
        {
            return original.CreateCachedFileUrl(filename);
        }

        public string CreateRawFileUrl(string filename)
        {
            return filename.TrimStart('~');
            //return original.CreateRawFileUrl(filename);
        }

        [Obsolete]
        public string CreateRawFileUrl(string filename, string hash)
        {
            return original.CreateRawFileUrl(filename, hash);
        }
    }
}
