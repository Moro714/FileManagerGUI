using System;
using System.IO;
using System.Linq; // Poate fi util pentru Linq, de exemplu la filtrare

namespace FileManagerGUI
{
    public static class FileOperations
    {
        /// <summary>
        /// Sterge un fisier sau un director (recursiv pentru directoare).
        /// </summary>
        /// <param name="path">Calea completa catre fisierul/directorul de sters.</param>
        public static void DeleteFileOrDirectory(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                // Al doilea parametru (true) indica stergerea recursiva a continutului
                Directory.Delete(path, true);
            }
            else
            {
                throw new FileNotFoundException($"Fisierul sau directorul '{path}' nu exista.");
            }
        }

        /// <summary>
        /// Redenumeste un fisier sau un director.
        /// </summary>
        /// <param name="oldPath">Calea completa curenta.</param>
        /// <param name="newPath">Calea completa noua (incluzand noul nume).</param>
        public static void Rename(string oldPath, string newPath)
        {
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath); // Metoda Move este folosita si pentru redenumire
            }
            else if (Directory.Exists(oldPath))
            {
                Directory.Move(oldPath, newPath); // Metoda Move este folosita si pentru redenumire
            }
            else
            {
                throw new FileNotFoundException($"Sursa '{Path.GetFileName(oldPath)}' nu exista.");
            }
        }

        /// <summary>
        /// Copiaza un fisier sau un director (recursiv pentru directoare).
        /// </summary>
        /// <param name="sourcePath">Calea completa a sursei.</param>
        /// <param name="destinationPath">Calea completa a destinatiei.</param>
        public static void Copy(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                // true permite suprascrierea fisierului destinatie daca exista deja
                File.Copy(sourcePath, destinationPath, true);
            }
            else if (Directory.Exists(sourcePath))
            {
                CopyDirectoryRecursive(sourcePath, destinationPath);
            }
            else
            {
                throw new FileNotFoundException($"Sursa '{Path.GetFileName(sourcePath)}' nu exista.");
            }
        }

        /// <summary>
        /// Muta un fisier sau un director.
        /// </summary>
        /// <param name="sourcePath">Calea completa a sursei.</param>
        /// <param name="destinationPath">Calea completa a destinatiei.</param>
        public static void Move(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                // Metoda Move se ocupa de mutare (inclusiv redenumire daca dest. e in acelasi dir)
                File.Move(sourcePath, destinationPath);
            }
            else if (Directory.Exists(sourcePath))
            {
                Directory.Move(sourcePath, destinationPath);
            }
            else
            {
                throw new FileNotFoundException($"Sursa '{Path.GetFileName(sourcePath)}' nu exista.");
            }
        }

        /// <summary>
        /// Creeaza un director nou.
        /// </summary>
        /// <param name="parentPath">Calea directorului parinte.</param>
        /// <param name="folderName">Numele noului director.</param>
        public static void CreateFolder(string parentPath, string folderName)
        {
            string newFolderPath = Path.Combine(parentPath, folderName);
            if (Directory.Exists(newFolderPath))
            {
                throw new InvalidOperationException($"Un director numit '{folderName}' exista deja in '{parentPath}'.");
            }
            Directory.CreateDirectory(newFolderPath);
        }

        /// <summary>
        /// Creeaza un fisier gol.
        /// </summary>
        /// <param name="parentPath">Calea directorului parinte.</param>
        /// <param name="fileName">Numele noului fisier.</param>
        public static void CreateFile(string parentPath, string fileName)
        {
            string newFilePath = Path.Combine(parentPath, fileName);
            if (File.Exists(newFilePath))
            {
                throw new InvalidOperationException($"Un fisier numit '{fileName}' exista deja in '{parentPath}'.");
            }
            File.WriteAllText(newFilePath, string.Empty); // Creeaza fisierul si il lasa gol
        }

        /// <summary>
        /// Metoda helper pentru copierea recursiva a directoarelor.
        /// </summary>
        private static void CopyDirectoryRecursive(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Directorul sursa nu exista sau nu poate fi gasit: {dir.FullName}");

            // Creaza directorul destinatie daca nu exista
            Directory.CreateDirectory(destinationDir);

            // Copiaza toate fisierele din directorul curent
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true); // true pentru a suprascrie daca exista
            }

            // Copiaza subdirectoarele recursiv
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectoryRecursive(subDir.FullName, newDestinationDir);
            }
        }
    }
}