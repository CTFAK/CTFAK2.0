using System.Collections.Generic;
using CTFAK.MFA;
using CTFAK.MFA.MFAObjectLoaders;
using JFusion.ObjectProperties;

namespace JFusion.ObjectTypes
{
    
    
   
    public class JMFAActive:JMFAObject
    {
        public Dictionary<int, JMFAAnimation> animations = new Dictionary<int, JMFAAnimation>();
        
        public static JMFAActive FromMFA(MFAObjectInfo oi)
        {
            
            var active = new JMFAActive();
            active.objectType = 2;
            var activeProps = (MFAActive)oi.Loader;
            foreach (var mfaAnim in activeProps.Items)
            {
                var newAnim = new JMFAAnimation();
                newAnim.name = mfaAnim.Value.Name;
                if (mfaAnim.Value.Directions.Count == 0) continue;
                foreach (var mfaDir in mfaAnim.Value.Directions)
                {
                    if (mfaDir.Frames.Count == 0) continue;
                    newAnim.directions.Add(new JMFAAnimationDirection()
                    {
                        minSpeed = mfaDir.MinSpeed,
                        maxSpeed = mfaDir.MaxSpeed,
                        backTo = mfaDir.BackTo,
                        repeat=mfaDir.Repeat,
                        frames=mfaDir.Frames
                    });
                }
                active.animations.Add(mfaAnim.Key,newAnim);
            }
            
            return active;
        }
        
    }
}