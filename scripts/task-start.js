const { execFileSync } = require('child_process');

// Get arguments passed to the npm script, ignoring 'node' and the script path.
const args = process.argv.slice(2);

// The arguments for the 'bd create' command.
const createArgs = [
    'create',
    '--silent',
    '--type',
    'task',
    '--priority',
    '2',
    ...args
];

try {
    // Run 'bd create' and capture the output (the bead ID).
    const beadId = execFileSync('bd', createArgs, { encoding: 'utf8' }).trim();

    if (!beadId) {
        throw new Error('Failed to create bead: "bd create" returned an empty ID.');
    }

    console.log(`Successfully created bead: ${beadId}`);

    // Run 'bd config set' to update the current bead.
    const configArgs = ['config', 'set', 'current_bead', beadId];
    execFileSync('bd', configArgs);

    console.log(`Successfully set current_bead to: ${beadId}`);

} catch (error) {
    console.error('Error in task:start script:', error.message);
    process.exit(1);
}
