package com.nitrorush;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.scheduling.annotation.EnableScheduling;

@SpringBootApplication
@EnableScheduling
public class NitroRushApplication {

    public static void main(String[] args) {
        SpringApplication.run(NitroRushApplication.class, args);
    }
}
