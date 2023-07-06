module.exports.readVersion = (contents) => {
    const lines = contents.split(/\r?\n/);
    const versionNumber = lines.filter(l => l.includes('<version>'))[0]
        .split('>')[1]
        .split('<')[0];

    return versionNumber;
};

module.exports.writeVersion = function (contents, version) {
    const updatedContents = contents
        .replace(/<veresion>*.*.*<\/veresion>/g,
            `<veresion>${version}</veresion>`);

    return updatedContents;
};