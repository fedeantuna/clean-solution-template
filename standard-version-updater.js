module.exports.readVersion = (contents) => {
    const lines = contents.split(/\r?\n/);
    const versionNumber = lines.filter(l => l.includes('<version>'))[0]
        .split('>')[1]
        .split('<')[0];

    return versionNumber;
};

module.exports.writeVersion = function (contents, version) {
    const updatedContents = contents
        .replace(/<version>*.*.*<\/version>/g,
            `<version>${version}</version>`);

    return updatedContents;
};