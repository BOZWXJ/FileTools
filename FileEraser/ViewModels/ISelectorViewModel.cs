using FileEraser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEraser.ViewModels
{
	public interface ISelectorViewModel
	{
		public IFileSelector GetFileSelector();
	}
}
