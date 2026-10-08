package com.nitrorush.garage;

import com.nitrorush.auth.Role;
import com.nitrorush.auth.User;
import com.nitrorush.auth.UserRepository;
import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.CommandLineRunner;
import org.springframework.context.annotation.Profile;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Component;

import java.util.List;

/** Seeds the catalog with three starter cars and optional demo accounts (via env). */
@Component
@Profile("!test")
public class DataSeeder implements CommandLineRunner {

    private static final Logger log = LoggerFactory.getLogger(DataSeeder.class);

    private final CarRepository carRepository;
    private final UserRepository userRepository;
    private final PlayerProfileRepository profileRepository;
    private final PasswordEncoder passwordEncoder;

    @Value("${nitrorush.demo.email:}")
    private String demoEmail;

    @Value("${nitrorush.demo.password:}")
    private String demoPassword;

    public DataSeeder(CarRepository carRepository, UserRepository userRepository,
                      PlayerProfileRepository profileRepository, PasswordEncoder passwordEncoder) {
        this.carRepository = carRepository;
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.passwordEncoder = passwordEncoder;
    }

    @Override
    public void run(String... args) {
        if (carRepository.count() == 0) {
            carRepository.saveAll(List.of(
                    new Car("Neon Spark", Rarity.COMMON, 48, 18, 14, 18, 55, 12_000),
                    new Car("Vortex GT", Rarity.COMMON, 55, 14, 12, 16, 60, 15_000),
                    new Car("Dune Hammer", Rarity.RARE, 62, 10, 8, 12, 50, 32_000)));
            log.info("Seeded starter car catalog");
        }
        if (!demoEmail.isBlank() && !demoPassword.isBlank()
                && userRepository.findByEmailIgnoreCase(demoEmail).isEmpty()) {
            User demo = userRepository.save(new User(demoEmail, passwordEncoder.encode(demoPassword), Role.PLAYER));
            profileRepository.save(new PlayerProfile(demo, "demo_player"));
            log.info("Seeded demo player account {}", demoEmail);
        }
    }
}
