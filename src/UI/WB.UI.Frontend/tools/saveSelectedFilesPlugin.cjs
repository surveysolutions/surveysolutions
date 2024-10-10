import fs from 'fs';
import path from 'path';

export default function saveSelectedFilesPlugin(options = {}) {
    const { filesToSave = [] } = options;
    let isFirstBuild = true;

    return {
        name: 'save-selected-files',
        apply: 'serve',
        enforce: 'pre',

        async handleHotUpdate(bundle) {
            //console.log(bundle)
            if (isFirstBuild) {
                for (const fileConfig of filesToSave) {
                    const { source, destination, name } = fileConfig;
                    if (path.basename(bundle.file) == path.basename(source)) {
                        const outputPath = path.join(destination, name);
                        const content = await bundle.read()

                        //console.log(`Readed: ${outputPath}`);

                        let modifiedContent = content;
                        if (path.extname(source) === '.html') {
                            modifiedContent = modifyHtmlContent(content);
                        }

                        await fs.writeFile(path.resolve(outputPath), modifiedContent, (err) => {
                            if (err) {
                                console.error('Error writing to file:', err);
                            } else {
                                console.log(`Saved: ${outputPath}`);
                            }
                        });

                    } else {
                        //console.warn(`File ${source} not found in the bundle.`);
                    }
                }
                isFirstBuild = true;
            }
        }
    };

    function modifyHtmlContent(content) {
        const scriptTagRegex = /<script\s+type=["']module["']\s+src=["'](\/src\/.*?main\.js)["']><\/script>/g;

        let newScripts = `<script type='module' src='/.vite/@@vite/client'></script>\n`;

        content = content.replace(scriptTagRegex, (match, src) => {
            const newSrc = `/.vite${src}`;
            newScripts += `<script type='module' src='${newSrc}'></script>\n`;
            return '';
        });

        const bodyTagRegex = /<body[^>]*>/i;
        content = content.replace(bodyTagRegex, (match) => `${match}\n${newScripts}`);

        return content;
    }
}