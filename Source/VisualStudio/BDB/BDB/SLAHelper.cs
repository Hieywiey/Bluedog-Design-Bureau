﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BDB
{
    public class ModuleBdbSLAHelper : PartModule
    {
        [KSPField]
        public string moduleID = "bdbSLAHelper";

        private bool decoupled = false;

        private ModuleDecouple decoupler;
        private ModuleDecouple payloadDecoupler;
        private List<ModuleDecouple> panels;

        [KSPField(isPersistant = false)]
        public string decouplerNodeID = "";

        [KSPField(isPersistant = false)]
        public string payloadDecouplerNodeID = "";

        [KSPField(isPersistant = false)]
        public string panelDecouplerNodeID = "";

        [KSPField(isPersistant = false)]
        public bool showPanelForcePercent = false;

        [KSPEvent(guiName = "Decouple", guiActive = true)]
        public void Decouple()
        {
            if (decoupler != null && !decoupler.isDecoupled)
                decoupler.Decouple();
            foreach (ModuleDecouple d in panels)
            {
                if (!d.isDecoupled)
                    d.Decouple();
            }
        }

        [KSPAction("Decouple")]
        public void DecoupleAction(KSPActionParam param)
        {
            Decouple();
        }

        public override void OnActive()
        {
            Decouple();
        }

        public void Start()
        {
            panels = new List<ModuleDecouple>();
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);

            List<ModuleDecouple> decouplers = new List<ModuleDecouple>();
            decouplers = this.GetComponents<ModuleDecouple>().ToList();
            foreach (ModuleDecouple d in decouplers)
            {
                if (decouplerNodeID != "" && d.explosiveNodeID == decouplerNodeID)
                {
                    decoupler = d;
                    decoupler.Actions["DecoupleAction"].active = false;
                    decoupler.Events["Decouple"].active = false;
                    decoupler.Events["ToggleStaging"].active = false;
                    decoupler.Actions["DecoupleAction"].guiName = "Decouple Top";
                    decoupler.Fields["ejectionForcePercent"].guiName = "Force Percent (Top)";
                }
                else if (payloadDecouplerNodeID != "" && d.explosiveNodeID == payloadDecouplerNodeID)
                {
                    payloadDecoupler = d;
                    payloadDecoupler.Events["Decouple"].guiName = "Decouple Payload";
                    payloadDecoupler.Actions["DecoupleAction"].guiName = "Decouple Payload";
                    payloadDecoupler.Fields["ejectionForcePercent"].guiName = "Force Percent (Payload)";
                }
                else if (panelDecouplerNodeID != "" && d.explosiveNodeID.StartsWith(panelDecouplerNodeID))
                {
                    panels.Add(d);
                    d.Actions["DecoupleAction"].active = false;
                    d.Events["Decouple"].active = false;
                    d.Events["ToggleStaging"].active = false;
                    d.Actions["DecoupleAction"].guiName = "Decouple Panel";
                    d.Fields["ejectionForcePercent"].guiName = "Force Percent (Panel)";
                    d.Fields["ejectionForcePercent"].guiActiveEditor = showPanelForcePercent;
                }
            }
            CheckDecoupled();
        }

        private void OnDestroy()
        {
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
        }

        private void OnVesselWasModified(Vessel v)
        {
            CheckDecoupled();
        }

        private void CheckDecoupled()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                decoupled = false;
                //if (decoupler != null && decoupler.isDecoupled)
                //    decoupled = true;
                foreach (ModuleDecouple d in panels)
                {
                    if (d.isDecoupled)
                        decoupled = true;
                }

                if (payloadDecoupler != null)
                    payloadDecoupler.isEnabled = decoupled;

                Events["Decouple"].active = !decoupled;
            }
        }
    }

}

