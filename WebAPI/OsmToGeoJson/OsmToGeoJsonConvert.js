const osmtogeojson = require('osmtogeojson');

let inputData = '';

process.stdin.on('data', chunk => {
    inputData += chunk;
});

process.stdin.on('end', () => {
    const osmData = JSON.parse(inputData);
    const geojson = osmtogeojson(osmData);
    process.stdout.write(JSON.stringify(geojson));
});