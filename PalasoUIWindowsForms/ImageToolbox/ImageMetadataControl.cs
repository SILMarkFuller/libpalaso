﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class ImageMetadataControl : UserControl
	{
		private PalasoImage _image;

		public ImageMetadataControl()
		{
			InitializeComponent();
			UpdateDisplay();
		}

		public void SetImage(PalasoImage image)
		{
			_image = image;
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			if(_image == null)
			{
				_illustrator.ReadOnly = _copyright.ReadOnly = _licenseDescription.Visible = false;
				return;
			}

			_lockedCheckbox.Checked = _image.MetaDataLocked;
			_illustrator.Text = _image.MetaData.AttributionName;
			_copyright.Text = _image.MetaData.CopyrightNotice;

			_illustrator.ReadOnly = _copyright.ReadOnly = _image.MetaDataLocked;

			//only handle the first one, for now
			if (FirstLicense ==null)
			{
				_licenseImage.Image = null;
				_licenseDescription.Visible = false;
			}
			else
			{
				_licenseDescription.ReadOnly = FirstLicense.EditingAllowed;
				_licenseDescription.Visible = true;
				_licenseDescription.Text = FirstLicense.GetDescription("en");
				_licenseImage.Image = FirstLicense.GetImage();
			}
		}
		private LicenseInfo FirstLicense
		{
			get
			{
				if (_image.MetaData.License!=null)
					return null;
				return _image.MetaData.License;
			}
		}

		private void _illustrator_TextChanged(object sender, EventArgs e)
		{
			_image.MetaData.AttributionName = _illustrator.Text;
		}

		private void _copyright_TextChanged(object sender, EventArgs e)
		{
			_image.MetaData.CopyrightNotice = _copyright.Text;
		}

		private void _lockedCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			_image.MetaDataLocked = _lockedCheckbox.Checked;
			UpdateDisplay();
		}

		private void _licenseDescription_TextChanged(object sender, EventArgs e)
		{
			FirstLicense.SetDescription("en",_licenseDescription.Text);
		}

		//TODO: need a simple chooser (combo box?) for Creative Commons or custom, if custom, show editable description.
	}
}
