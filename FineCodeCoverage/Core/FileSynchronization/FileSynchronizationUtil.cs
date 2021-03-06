﻿using System.IO;
using System.Linq;
using FineCodeCoverage.Engine.Utilities;

namespace FineCodeCoverage.Engine.FileSynchronization
{
	internal static partial class FileSynchronizationUtil
	{
		public static void Synchronize(string sourceFolder, string destinationFolder)
		{
			var srceDir = new DirectoryInfo(Path.GetFullPath(sourceFolder) + '\\');
			var destDir = new DirectoryInfo(Path.GetFullPath(destinationFolder) + '\\');

			// file lists

			var srceFiles = srceDir.GetFiles("*", SearchOption.AllDirectories).AsParallel().Select(fi => new ComparableFile(fi, fi.FullName.Substring(srceDir.FullName.Length)));
			ParallelQuery<ComparableFile> destFiles() => destDir.GetFiles("*", SearchOption.AllDirectories).AsParallel().Select(fi => new ComparableFile(fi, fi.FullName.Substring(destDir.FullName.Length)));

			// copy to dest

			foreach (var fileToCopy in srceFiles.Except(destFiles(), FileComparer.Singleton))
			{
				var to = new FileInfo(fileToCopy.FileInfo.FullName.Replace(srceDir.FullName, destDir.FullName));

				if (!to.Directory.Exists)
				{
					Directory.CreateDirectory(to.DirectoryName);
				}

				File.Copy(fileToCopy.FileInfo.FullName, to.FullName, true);
			}

			// delete from dest

			foreach (var fileToDelete in destFiles().Except(srceFiles, FileComparer.Singleton))
			{
				File.Delete(fileToDelete.FileInfo.FullName);
			}
		}
	}
}
