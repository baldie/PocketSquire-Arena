const { execFileSync } = require('child_process');

const isWin = process.platform === 'win32';
const bdCmd = isWin ? 'bd.exe' : 'bd';

// Get arguments passed to the npm script, ignoring 'node' and the script path.
const args = process.argv.slice(2);
const title = args.join(' ');

if (!title) {
    console.error('Usage: npm run task:start "Task Title"');
    process.exit(1);
}

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
    const beadId = execFileSync(bdCmd, createArgs, {
        encoding: 'utf8',
        shell: isWin
    }).trim();

    if (!beadId) {
        throw new Error('Failed to create bead: "bd create" returned an empty ID.');
    }

    console.log(`Successfully created bead: ${beadId}`);

    // Run 'bd config set' to update the current bead.
    const configArgs = ['config', 'set', 'current_bead', beadId];
    execFileSync(bdCmd, configArgs, { shell: isWin });

    console.log(`Successfully set current_bead to: ${beadId}`);

    // Create a slug from the title.
    const slug = title.toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/(^-|-$)/g, '');

    const branchName = `task/${beadId}-${slug}`;

    console.log(`Creating branch: ${branchName}`);

    // Create and switch to the new branch.
    // Use git checkout -b which works on both Windows and Linux.
    execFileSync('git', ['checkout', '-b', branchName], { shell: isWin });

    console.log(`Switched to branch: ${branchName}`);

} catch (error) {
    console.error('Error in task:start script:', error.message);
    process.exit(1);
}
