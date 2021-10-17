import express from 'express';
import path from 'path';
import fs from 'fs';
import dotenv from 'dotenv';
import os from 'os';
import cors from 'cors';


let config = {
    HOSTNAME: os.hostname()
};
for (const [key, value] of Object.entries(process.env)) {
    if (key.startsWith('ZENO_TORRENT_DAEMON')) {
        config[key] = value;
    }
}
dotenv.config();
for (const [key, value] of Object.entries(process.env)) {
    if (key.startsWith('ZENO_TORRENT_DAEMON')) {
        config[key] = value;
    }
}
console.log(config);

const indexHtmlPath = path.join(path.resolve(path.dirname('')), 'build', 'index.html');
const replaceTag = 'window.ENV={}';
const fd = fs.openSync(indexHtmlPath);
let data = fs.readFileSync(fd, 'utf8');
data = data.replace(replaceTag, 'window.ENV='+JSON.stringify(config));
fs.writeFileSync(indexHtmlPath, data);
fs.closeSync(fd);

const app = express();

app.use(cors());

app.get('/health', (req, res) => res.send('Healthy'));
app.use(express.static(path.join(path.resolve(path.dirname('')), 'build')));
app.get('*', (request, response) => response.sendFile(indexHtmlPath));

app.listen(process.env.WEB_PORT, () => console.log(`Serving on ${process.env.WEB_PORT}`));