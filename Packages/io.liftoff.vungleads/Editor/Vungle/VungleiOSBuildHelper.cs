using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Collections.Generic;
using System.IO;

namespace VungleAds
{
    public class VungleiOSPostBuilder
    {
    #if UNITY_IOS
        // Updated 2026-02-02 from https://vungle-static-assets.s3.amazonaws.com/dashboard/admin/prod/skadnetworkids.xml
        private static readonly string[] SKAdNetworks = new string[]
        {
            "2fnua5tdw4.skadnetwork",
            "5lm9lj6jb7.skadnetwork",
            "54nzkqm89y.skadnetwork",
            "44n7hlldy6.skadnetwork",
            "5tjdwbrq8w.skadnetwork",
            "6964rsfnh4.skadnetwork",
            "6g9af3uyq4.skadnetwork",
            "6v7lgmsu45.skadnetwork",
            "7fmhfwg9en.skadnetwork",
            "a7xqa6mtl2.skadnetwork",
            "a8cz6cu7e5.skadnetwork",
            "84993kbrcf.skadnetwork",
            "bxvub5ada5.skadnetwork",
            "c6k4g5qg8m.skadnetwork",
            "4dzt52r2t5.skadnetwork",
            "79pbpufp6p.skadnetwork",
            "9t245vhmpl.skadnetwork",
            "9vvzujtq5s.skadnetwork",
            "7rz58n8ntl.skadnetwork",
            "av6w8kgt66.skadnetwork",
            "3qcr597p9d.skadnetwork",
            "737z793b9f.skadnetwork",
            "89z7zv988g.skadnetwork",
            "9rd848q2bz.skadnetwork",
            "c3frkrj4fj.skadnetwork",
            "22mmun2rn5.skadnetwork",
            "275upjj5gd.skadnetwork",
            "3l6bd9hu43.skadnetwork",
            "238da6jt44.skadnetwork",
            "3rd42ekr43.skadnetwork",
            "4w7y6s5ca2.skadnetwork",
            "52fl2v3hgk.skadnetwork",
            "578prtvx9j.skadnetwork",
            "6p4ks3rnbw.skadnetwork",
            "97r2b46745.skadnetwork",
            "apzhy3va96.skadnetwork",
            "44jx6755aq.skadnetwork",
            "9b89h5y424.skadnetwork",
            "9nlqeag3gk.skadnetwork",
            "523jb4fst2.skadnetwork",
            "b9bk5wbcq9.skadnetwork",
            "3qy4746246.skadnetwork",
            "3sh42y64q3.skadnetwork",
            "4fzdc2evr5.skadnetwork",
            "5l3tpt7t6e.skadnetwork",
            "cg4yq2srnc.skadnetwork",
            "294l99pt4k.skadnetwork",
            "32z4fx6l9h.skadnetwork",
            "5a6flpkh64.skadnetwork",
            "cj5566h2ga.skadnetwork",
            "2u9pt9hc89.skadnetwork",
            "424m5254lk.skadnetwork",
            "7ug5zh24hu.skadnetwork",
            "24zw6aqk47.skadnetwork",
            "74b6s63p6l.skadnetwork",
            "8m87ys6875.skadnetwork",
            "488r3q3dtq.skadnetwork",
            "4pfyvq9l8r.skadnetwork",
            "24t9a8vw3c.skadnetwork",
            "4468km3ulz.skadnetwork",
            "8s468mfl3y.skadnetwork",
            "6xzpu9s2p8.skadnetwork",
            "cs644xg564.skadnetwork",
            "dbu4b84rxf.skadnetwork",
            "n9x2a789qt.skadnetwork",
            "ludvb6z3bs.skadnetwork",
            "f38h382jlk.skadnetwork",
            "s39g8k73mm.skadnetwork",
            "x44k69ngh6.skadnetwork",
            "ejvt5qm6ak.skadnetwork",
            "g6gcrrvk4p.skadnetwork",
            "mp6xlyr22a.skadnetwork",
            "mtkv5xtk9e.skadnetwork",
            "uw77j35x4d.skadnetwork",
            "w9q455wk68.skadnetwork",
            "m8dbw4sv7c.skadnetwork",
            "e5fvkxwrpn.skadnetwork",
            "cstr6suwn9.skadnetwork",
            "k674qkevps.skadnetwork",
            "kbmxgpxpgc.skadnetwork",
            "f73kdq92p3.skadnetwork",
            "hb56zgv37p.skadnetwork",
            "hs6bdukanm.skadnetwork",
            "ppxm28t8ap.skadnetwork",
            "pwdxu55a5a.skadnetwork",
            "r45fhb6rf7.skadnetwork",
            "t6d3zquu66.skadnetwork",
            "wzmmz9fp6w.skadnetwork",
            "feyaarzu9v.skadnetwork",
            "u679fj5vs4.skadnetwork",
            "dzg6xy7pwj.skadnetwork",
            "g28c52eehv.skadnetwork",
            "klf5c3l5u5.skadnetwork",
            "mls7yz5dvl.skadnetwork",
            "p78axxw29g.skadnetwork",
            "vcra2ehyfk.skadnetwork",
            "glqzh8vgby.skadnetwork",
            "pwa73g5rt2.skadnetwork",
            "rx5hdcabgc.skadnetwork",
            "v72qych5uu.skadnetwork",
            "qwpu75vrh2.skadnetwork",
            "tl55sbb4fm.skadnetwork",
            "ydx93a7ass.skadnetwork",
            "rvh3l7un93.skadnetwork",
            "x5l83yy675.skadnetwork",
            "kbd757ywx3.skadnetwork",
            "m5mvw97r93.skadnetwork",
            "prcb7njmu6.skadnetwork",
            "v79kvwwj4g.skadnetwork",
            "xy9t38ct57.skadnetwork",
            "g2y4y55b64.skadnetwork",
            "gta9lk7p23.skadnetwork",
            "n6fk4nfna4.skadnetwork",
            "qu637u8glc.skadnetwork",
            "t38b2kh725.skadnetwork",
            "wg4vff78zm.skadnetwork",
            "y5ghdn5j9k.skadnetwork",
            "mj797d8u6f.skadnetwork",
            "mlmmfzh3r3.skadnetwork",
            "y45688jllp.skadnetwork",
            "ggvn48r87g.skadnetwork",
            "m297p6643m.skadnetwork",
            "mqn7fxpca7.skadnetwork",
            "qqp299437r.skadnetwork",
            "hdw39hrw9y.skadnetwork",
            "krvm3zuq6h.skadnetwork",
            "yclnxrl5pm.skadnetwork",
            "zq492l623r.skadnetwork",
            "zmvfpc5aq8.skadnetwork",
            "cwn433xbcr.skadnetwork",
            "vhf287vqwu.skadnetwork",
            "xga6mpmplv.skadnetwork",
            "cp8zw746q7.skadnetwork",
            "v9wttpbfk9.skadnetwork",
            "ns5j362hk7.skadnetwork",
            "su67r6k2v3.skadnetwork",
            "z959bm4gru.skadnetwork",
            "x8uqf25wch.skadnetwork",
            "lr83yxwka7.skadnetwork",
            "n38lu8286q.skadnetwork",
            "fz2k2k5tej.skadnetwork",
            "ecpz2srf59.skadnetwork",
            "bvpn9ufa9b.skadnetwork",
            "6rd35atwn8.skadnetwork",
            "ln5gz23vtd.skadnetwork",
            "tmhh9296z4.skadnetwork",
            "k6y4y55b64.skadnetwork",
            "sczv5946wb.skadnetwork",
            "a2p9lx4jpn.skadnetwork",
            "6yxyv74ff7.skadnetwork",
            "f7s53z58qe.skadnetwork",
            "dkc879ngq3.skadnetwork",
            "87u5trcl3r.skadnetwork",
            "fq6vru337s.skadnetwork",
            "f2zub97jtl.skadnetwork",
        };

        // Updated 2026-02-02 from https://vungle-static-assets.s3.amazonaws.com/dashboard/admin/prod/skadnetworkids.xml
        private static readonly string[] AdAttributionKitNetworks = new string[]
        {
            "thzdn4h5nc.adattributionkit",
            "raa6f494kr.adattributionkit",
            "6lz2ygh3q6.adattributionkit",
            "m2jqnlggk3.adattributionkit",
            "pg7ctvrt6f.adattributionkit",
            "77y3x8wds4.adattributionkit",
        };

        [PostProcessBuild(800)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS)
                return;

            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;

            PlistElementArray skAdNetworkItems = rootDict.values.ContainsKey("SKAdNetworkItems")
                ? rootDict["SKAdNetworkItems"].AsArray()
                : rootDict.CreateArray("SKAdNetworkItems");

            foreach (string id in SKAdNetworks)
                AddNetworkIdentifier(skAdNetworkItems, "SKAdNetworkIdentifier", id);

            PlistElementArray aakItems = rootDict.values.ContainsKey("AdAttributionKitNetworkItems")
                ? rootDict["AdAttributionKitNetworkItems"].AsArray()
                : rootDict.CreateArray("AdAttributionKitNetworkItems");

            foreach (string id in AdAttributionKitNetworks)
                AddNetworkIdentifier(aakItems, "AdNetworkIdentifier", id);

            File.WriteAllText(plistPath, plist.WriteToString());
        }

        private static void AddNetworkIdentifier(PlistElementArray array, string key, string value)
        {
            foreach (var item in array.values)
            {
                var dict = item.AsDict();
                if (dict != null && dict.values.ContainsKey(key) && dict[key].AsString() == value)
                    return;
            }
            array.AddDict().SetString(key, value);
        }
    #endif
    }
}
