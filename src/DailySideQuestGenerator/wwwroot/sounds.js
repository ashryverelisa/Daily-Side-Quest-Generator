// Sound effects using Web Audio API for RPG-style progression feedback
window.playSound = function(soundName) {
    const audioContext = new (window.AudioContext || window.webkitAudioContext)();
    
    switch(soundName) {
        case 'xp-gain':
            playXpGainSound(audioContext);
            break;
        case 'level-up':
            playLevelUpSound(audioContext);
            break;
        case 'quest-complete':
            playQuestCompleteSound(audioContext);
            break;
    }
};

function playXpGainSound(ctx) {
    // Quick ascending chime for XP gain
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    
    osc.connect(gain);
    gain.connect(ctx.destination);
    
    osc.type = 'sine';
    osc.frequency.setValueAtTime(523.25, ctx.currentTime); // C5
    osc.frequency.exponentialRampToValueAtTime(783.99, ctx.currentTime + 0.1); // G5
    
    gain.gain.setValueAtTime(0.3, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.2);
    
    osc.start(ctx.currentTime);
    osc.stop(ctx.currentTime + 0.2);
}

function playLevelUpSound(ctx) {
    // Triumphant fanfare for level up
    const notes = [
        { freq: 523.25, time: 0, duration: 0.15 },      // C5
        { freq: 659.25, time: 0.12, duration: 0.15 },   // E5
        { freq: 783.99, time: 0.24, duration: 0.15 },   // G5
        { freq: 1046.50, time: 0.36, duration: 0.4 },   // C6 (hold)
    ];
    
    notes.forEach(note => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(note.freq, ctx.currentTime + note.time);
        
        gain.gain.setValueAtTime(0, ctx.currentTime + note.time);
        gain.gain.linearRampToValueAtTime(0.4, ctx.currentTime + note.time + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + note.time + note.duration);
        
        osc.start(ctx.currentTime + note.time);
        osc.stop(ctx.currentTime + note.time + note.duration + 0.1);
    });
    
    // Add a shimmering effect
    for (let i = 0; i < 3; i++) {
        const shimmer = ctx.createOscillator();
        const shimmerGain = ctx.createGain();
        
        shimmer.connect(shimmerGain);
        shimmerGain.connect(ctx.destination);
        
        shimmer.type = 'sine';
        shimmer.frequency.setValueAtTime(2000 + (i * 500), ctx.currentTime + 0.5 + (i * 0.05));
        
        shimmerGain.gain.setValueAtTime(0.1, ctx.currentTime + 0.5 + (i * 0.05));
        shimmerGain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.8 + (i * 0.05));
        
        shimmer.start(ctx.currentTime + 0.5 + (i * 0.05));
        shimmer.stop(ctx.currentTime + 0.9 + (i * 0.05));
    }
}

function playQuestCompleteSound(ctx) {
    // Satisfying completion chime
    const notes = [
        { freq: 659.25, time: 0, duration: 0.1 },      // E5
        { freq: 783.99, time: 0.08, duration: 0.2 },   // G5
    ];
    
    notes.forEach(note => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        osc.type = 'sine';
        osc.frequency.setValueAtTime(note.freq, ctx.currentTime + note.time);
        
        gain.gain.setValueAtTime(0.25, ctx.currentTime + note.time);
        gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + note.time + note.duration);
        
        osc.start(ctx.currentTime + note.time);
        osc.stop(ctx.currentTime + note.time + note.duration + 0.05);
    });
}

