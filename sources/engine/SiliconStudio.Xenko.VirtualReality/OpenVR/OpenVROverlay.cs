// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class OpenVROverlay : VROverlay
    {
        private ulong overlayId;

        public OpenVROverlay()
        {
            overlayId = OpenVR.CreateOverlay();
            if(overlayId == 0)
            {
                throw new System.Exception("Failed to create OpenVR overlay.");
            }

            OpenVR.InitOverlay(overlayId);
            OpenVR.SetOverlayEnabled(overlayId, true);
        }

        public override void Dispose()
        {           
        }

        private bool enabled = true;

        public override bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
                OpenVR.SetOverlayEnabled(overlayId, enabled);
            }
        }

        public override void UpdateSurface(CommandList commandList, Texture texture)
        {
            var pose = Matrix.Translation(Position) * Matrix.RotationQuaternion(Rotation);
            OpenVR.SetOverlayParams(overlayId, pose, FollowHeadRotation, SurfaceSize);
            OpenVR.SubmitOverlay(overlayId, texture);
        }
    }
}

#endif
