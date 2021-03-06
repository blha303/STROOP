﻿using SM64_Diagnostic.Structs;
using SM64_Diagnostic.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SM64_Diagnostic.Controls;
using SM64_Diagnostic.Extensions;

namespace SM64_Diagnostic.ManagerClasses
{
    public class ObjectManager : DataManager
    {
        List<WatchVariableControl> _behaviorDataControls = new List<WatchVariableControl>();
        ObjectAssociations _objAssoc;
        ObjectDataGui _objGui;

        object _watchVarLocker = new object();

        uint _currentAddress;
        int _slotIndex;
        string _slotPos;
        uint _behavior;
        bool _unclone = false;

        #region Fields
        public void SetBehaviorWatchVariables(List<WatchVariable> value, Color color)
        {
            lock (_watchVarLocker)
            {
                // Remove old watchVars from list
                foreach (var watchVar in _behaviorDataControls)
                {
                    _dataControls.Remove(watchVar);
                    _objGui.ObjectFlowLayout.Controls.Remove(watchVar.Control);
                }
                _behaviorDataControls.Clear();

                // Add new watchVars
                foreach (var watchVar in value)
                {
                    var newWatchVarControl = new WatchVariableControl(_stream, watchVar);
                    newWatchVarControl.Color = color;
                    _behaviorDataControls.Add(newWatchVarControl);
                    _dataControls.Add(newWatchVarControl);
                    _objGui.ObjectFlowLayout.Controls.Add(newWatchVarControl.Control);
                }
                _behaviorDataControls.ForEach(w => w.OtherOffset = _currentAddress);
            }
        }

        public uint? CurrentAddress
        {
            get
            {
                return _currentAddress;
            }
            set
            {
                if (_currentAddress != value)
                {
                    _currentAddress = value.HasValue ? value.Value : 0x0000;
                    _objGui.ObjAddressLabelValue.Text = "0x" + _currentAddress.ToString("X8");
                    foreach (WatchVariableControl watchVar in _dataControls)
                    {
                        watchVar.OtherOffset = _currentAddress;
                    }
                }
            }
        }

        public int SlotIndex
        {
            get
            {
                return _slotIndex;
            }
            set
            {
                if (_slotIndex != value)
                {
                    _slotIndex = value;
                    _objGui.ObjSlotIndexLabel.Text = _slotIndex.ToString();
                }
            }
        }

        public string SlotPos
        {
            get
            {
                return _slotPos;
            }
            set
            {
                if (_slotPos != value)
                {
                    _slotPos = value;
                    _objGui.ObjSlotPositionLabel.Text = _slotPos;
                }
            }
        }

        public uint Behavior
        {
            get
            {
                return _behavior;
            }
            set
            {
                if (_behavior != value)
                {
                    _behavior = value;
                    _objGui.ObjBehaviorLabel.Text = "0x" + _behavior.ToString("X4");
                }
            }
        }

        public string Name
        {
            get
            {
                return _objGui.ObjectNameTextBox.Text;
            }
            set
            {
                if (_objGui.ObjectNameTextBox.Text != value)
                    _objGui.ObjectNameTextBox.Text = value;
            }
        }

        public Color BackColor
        {
            set
            {
                if (_objGui.ObjectBorderPanel.BackColor != value)
                {
                    _objGui.ObjectBorderPanel.BackColor = value;
                    _objGui.ObjectImagePictureBox.BackColor = value.Lighten(0.5);
                }
            }
            get
            {
                return _objGui.ObjectBorderPanel.BackColor;
            }
        }

        public Image Image
        {
            get
            {
                return _objGui.ObjectImagePictureBox.Image;
            }
            set
            {
                if (_objGui.ObjectImagePictureBox.Image != value)
                    _objGui.ObjectImagePictureBox.Image = value;
            }
        }

        #endregion

        protected override void InitializeSpecialVariables()
        {
            _specialWatchVars = new List<DataContainer>()
            {
                new DataContainer("MarioDistanceToObject"),
                new DataContainer("MarioLateralDistanceToObject"),
                new DataContainer("MarioDistanceToObjectHome"),
                new DataContainer("MarioLateralDistanceToObjectHome"),
                new DataContainer("ObjectDistanceToHome"),
                new DataContainer("LateralObjectDistanceToHome"),
                new DataContainer("RngCallsPerFrame"),
            };
        }

        public ObjectManager(ProcessStream stream, ObjectAssociations objAssoc, List<WatchVariable> objectData, ObjectDataGui objectGui)
            : base(stream, objectData, objectGui.ObjectFlowLayout)
        { 
            _objGui = objectGui;
            _objAssoc = objAssoc;

            // Register controls on the control (for drag-and-drop)
            RegisterControlEvents(_objGui.ObjectBorderPanel);
            foreach (Control control in _objGui.ObjectBorderPanel.Controls)
                RegisterControlEvents(control);
            
            _objGui.ObjAddressLabelValue.Click += ObjAddressLabel_Click;
            _objGui.ObjAddressLabel.Click += ObjAddressLabel_Click;

            // Register buttons
            objectGui.CloneButton.Click += CloneButton_Click;
            objectGui.UnloadButton.Click += UnloadButton_Click;
            objectGui.MoveMarioToButton.Click += MoveMarioToButton_Click;
            objectGui.MoveToMarioButton.Click += MoveToMarioButton_Click;
        }

        private void ObjAddressLabel_Click(object sender, EventArgs e)
        {
            var variableInfo = new VariableViewerForm("Object Address", "Object",
                String.Format("0x{0:X8}", _currentAddress), String.Format("0x{0:X8}", (_currentAddress & 0x0FFFFFFF) + _stream.ProcessMemoryOffset));
            variableInfo.ShowDialog();
        }

        private void MoveToMarioButton_Click(object sender, EventArgs e)
        {
            if (!CurrentAddress.HasValue)
                return;
            MarioActions.MoveObjectToMario(_stream, CurrentAddress.Value);
        }

        private void MoveMarioToButton_Click(object sender, EventArgs e)
        {
            if (!CurrentAddress.HasValue)
                return;
            MarioActions.MoveMarioToObject(_stream, CurrentAddress.Value);
        }

        private void UnloadButton_Click(object sender, EventArgs e)
        {
            if (!CurrentAddress.HasValue)
                return;
            MarioActions.UnloadObject(_stream, CurrentAddress.Value);
        }

        private void CloneButton_Click(object sender, EventArgs e)
        {
            if (!CurrentAddress.HasValue)
                return;
            if (_unclone)
                MarioActions.UnCloneObject(_stream, CurrentAddress.Value);
            else
                MarioActions.CloneObject(_stream, CurrentAddress.Value);
        }

        private void ProcessSpecialVars()
        {
            // Get Mario position
            float mX, mY, mZ;
            mX = _stream.GetSingle(Config.Mario.StructAddress + Config.Mario.XOffset);
            mY = _stream.GetSingle(Config.Mario.StructAddress + Config.Mario.YOffset);
            mZ = _stream.GetSingle(Config.Mario.StructAddress + Config.Mario.ZOffset);

            // Get object position
            float objX, objY, objZ;
            objX = _stream.GetSingle(_currentAddress + Config.ObjectSlots.ObjectXOffset);
            objY = _stream.GetSingle(_currentAddress + Config.ObjectSlots.ObjectYOffset);
            objZ = _stream.GetSingle(_currentAddress + Config.ObjectSlots.ObjectZOffset);

            // Get object position
            float objHomeX, objHomeY, objHomeZ;
            objHomeX = _stream.GetSingle(_currentAddress + Config.ObjectSlots.HomeXOffset);
            objHomeY = _stream.GetSingle(_currentAddress + Config.ObjectSlots.HomeYOffset);
            objHomeZ = _stream.GetSingle(_currentAddress + Config.ObjectSlots.HomeZOffset);

            foreach (DataContainer specialVar in _specialWatchVars)
            {
                switch (specialVar.SpecialName)
                {
                    case "MarioDistanceToObject":
                        specialVar.Text = MoreMath.DistanceTo(mX, mY, mZ, objX, objY, objZ).ToString();
                        break;

                    case "MarioLateralDistanceToObject":
                        specialVar.Text = MoreMath.DistanceTo(mX, mZ, objX, objZ).ToString();
                        break;

                    case "MarioDistanceToObjectHome":
                        specialVar.Text = MoreMath.DistanceTo(mX, mY, mZ, objHomeX, objHomeY, objHomeZ).ToString();
                        break;

                    case "MarioLateralDistanceToObjectHome":
                        specialVar.Text = MoreMath.DistanceTo(mX, mZ, objHomeX, objHomeZ).ToString();
                        break;

                    case "ObjectDistanceToHome":
                        specialVar.Text = MoreMath.DistanceTo(objX, objY, objZ, objHomeX, objHomeY, objHomeZ).ToString();
                        break;

                    case "LateralObjectDistanceToHome":
                        specialVar.Text = MoreMath.DistanceTo(objX, objZ, objHomeX, objHomeZ).ToString();
                        break;

                    case "RngCallsPerFrame":
                        specialVar.Text = GetNumRngCalls().ToString();
                        break;
                }
            }
        }

        public override void Update(bool updateView)
        {
            if (!updateView)
                return;

            // Determine which object is being held
            uint holdingObj = _stream.GetUInt32(Config.Mario.StructAddress + Config.Mario.HoldingObjectPointerOffset);

            // Change to unclone if we are already holding the object
            if ((holdingObj == _currentAddress) != _unclone)
            {
                _unclone = !_unclone;

                // Update button text
                _objGui.CloneButton.Text = _unclone ? "UnClone" : "Clone";
            }

            base.Update(updateView);
            ProcessSpecialVars();
        }

        private int GetNumRngCalls()
        {
            var numberOfRngObjs = BitConverter.ToUInt32(_stream.ReadRam(Config.RngRecordingAreaAddress, 4), 0);

            int numOfCalls = 0;

            for (int i = 0; i < numberOfRngObjs; i++)
            {
                uint rngStructAdd = (uint)(Config.RngRecordingAreaAddress + 0x10 + 0x08 * i);
                var address = BitConverter.ToUInt32(_stream.ReadRam(rngStructAdd + 0x04, 4), 0);
                if (address != _currentAddress)
                    continue;

                var preRng = BitConverter.ToUInt16(_stream.ReadRam(rngStructAdd + 0x00, 2), 0);
                var postRng = BitConverter.ToUInt16(_stream.ReadRam(rngStructAdd + 0x02, 2), 0);

                numOfCalls = RngIndexer.GetRngIndexDiff(preRng, postRng);
                break;
            }

            return numOfCalls;
        }

        private void RegisterControlEvents(Control control)
        {
            control.AllowDrop = true;
            control.DragEnter += DragEnter;
            control.DragDrop += OnDrop;
            control.MouseDown += OnDrag;
        }

        private void OnDrag(object sender, EventArgs e)
        {
            if (!CurrentAddress.HasValue)
                return;

            // Start the drag and drop but setting the object slot index in Drag and Drop data
            var dropAction = new DropAction(DropAction.ActionType.Object, CurrentAddress.Value);
            (sender as Control).DoDragDrop(dropAction, DragDropEffects.All);
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            // Make sure we have valid Drag and Drop data (it is an index)
            if (!e.Data.GetDataPresent(typeof(DropAction)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            var dropAction = ((DropAction)e.Data.GetData(typeof(DropAction))).Action;
            if (dropAction != DropAction.ActionType.Object && dropAction != DropAction.ActionType.Mario)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Move;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            // Make sure we have valid Drag and Drop data (it is an index)
            if (!e.Data.GetDataPresent(typeof(DropAction)))
                return;

            var dropAction = ((DropAction)e.Data.GetData(typeof(DropAction)));
        }
    }
}
