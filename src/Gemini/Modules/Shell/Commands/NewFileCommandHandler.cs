﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using Gemini.Framework.Threading;

namespace Gemini.Modules.Shell.Commands
{
    [CommandHandler]
    public class NewFileCommandHandler : ICommandListHandler<NewFileCommandListDefinition>
    {
        private int _newFileCounter = 1;

        private readonly IShell _shell;
        private readonly IEditorProvider[] _editorProviders;

        [ImportingConstructor]
        public NewFileCommandHandler(
            IShell shell,
            [ImportMany] IEditorProvider[] editorProviders)
        {
            _shell = shell;
            _editorProviders = editorProviders;
        }

        public void Populate(Command command, List<Command> commands)
        {
            foreach (var editorProvider in _editorProviders)
                foreach (var editorFileType in editorProvider.FileTypes)
                    commands.Add(new Command(command.CommandDefinition)
                    {
                        Text = editorFileType.Name,
                        Tag = new NewFileTag
                        {
                            EditorProvider = editorProvider,
                            FileType = editorFileType
                        }
                    });
        }

        public async Task Run(Command command)
        {
            var tag = (NewFileTag) command.Tag;
            var newDocument = await tag.EditorProvider.CreateNew("Untitled " + (_newFileCounter++) + tag.FileType.FileExtension);
            _shell.OpenDocument(newDocument);
        }

        private class NewFileTag
        {
            public IEditorProvider EditorProvider;
            public EditorFileType FileType;
        }
    }
}